using Random_Experiment_Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using static Random_Experiment_Server.ServerMain;

namespace Random_Experiment_Server.WCF
{
    public class AuthenticatedToken
    {
        public string Token { get; set; }
        public string IP { get; set; }
        public DateTime Time { get; set; }
    }

    [ServiceBehavior(Namespace = "", ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RandomServer : IRandomServer
    {
        public static string GetIP()
        {
            try
            {
                DateTime cut = DateTime.UtcNow - TimeSpan.FromDays(7);
                ServerMain.Instance.myStats.RemoveAll(p => p.time < cut);

                OperationContext context = OperationContext.Current;
                MessageProperties prop = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

                return endpoint.Address;
            }
            catch { return ".unknown."; }
        }

        public static bool CheckDDOS(string ip)
        {
            DateTime cutOff = DateTime.UtcNow.AddSeconds(-5);
            ServerMain.DDOSlist.RemoveAll(p => (DateTime)p.Item2 < cutOff);

            if (ServerMain.DDOSlist.Count(p => (string)p.Item1 == ip) > 10)
            {
                Supporting.WriteLog($"[{ip}]:DDOS triggered");
                return true;
            }

            ServerMain.DDOSlist.Add(new Tuple<string, DateTime>(ip, DateTime.UtcNow));
            ServerMain.Instance.myStats.Add(new ServerMain.ServerStats { ip = ip, served = ServerMain.Served.DDOS, time = DateTime.UtcNow });

            return false;
        }

        public string GetToken()
        {
            string ip = GetIP();
            if (CheckDDOS(ip)) return null;

            string sessionToken = Convert.ToBase64String(this.MakeRandomSecret());
            DateTime now = DateTime.UtcNow;
            TimeSpan cut = new TimeSpan(0, 2, 0);
            if (ServerMain.Instance.Sessions.Exists(p => p.IP == ip && (now - p.Time < cut)))
                return "error:green";

            ServerMain.Instance.Sessions.RemoveAll(p => p.IP == ip);
            this.AddToken(new AuthenticatedToken() { IP = ip, Token = sessionToken, Time = DateTime.UtcNow });

            return sessionToken;
        }

        public string SubmitStatus(string token, SQLData data)
        {
            string ip = GetIP();
            if (CheckDDOS(ip)) return null;

            if (data == null) return null;

            if (!ServerMain.Instance.Sessions.Exists(p => p.IP == ip && p.Token == token))
                return "error:no token";

            AuthenticatedToken current = ServerMain.Instance.Sessions.Find(p => p.Token == token);
            ServerMain.Instance.Sessions.RemoveAll(p => p.IP == ip);

            if (data.Count < 2000)
                return "error:green";

            if (DateTime.UtcNow.Ticks - current.Time.Ticks < 0 || DateTime.UtcNow - current.Time > new TimeSpan(0, 2, 0))
                return "error:green";

            if (DateTime.UtcNow.Ticks - data.Time.Ticks < 0 || DateTime.UtcNow - data.Time > new TimeSpan(0, 2, 0))
                return "error:green";

            if (ServerMain.Instance.mySQL.AddData(data).IndexOf("error") != -1)
                return "error:SQL failed";

            Supporting.WriteLog($"[{ip}][{data.User.Substring(0, 8)}]:Added data[{data.Mean.ToString("N4")}][{data.Median.ToString("N4")}][{data.StdDev.ToString("N4")}][{data.Active}][{data.Count}]");
            ServerMain.Instance.myStats.Add(new ServerMain.ServerStats { ip = ip, served = ServerMain.Served.Submit, time = DateTime.UtcNow });

            return "success";
        }

        public void AddToken(AuthenticatedToken token)
        {
            string ip = GetIP();
            if (CheckDDOS(ip)) return;

            ServerMain.Instance.Sessions.Add(token);

            Supporting.WriteLog($"[{token.IP}]:Added token");
            ServerMain.Instance.myStats.Add(new ServerMain.ServerStats { ip = ip, served = ServerMain.Served.Token, time = DateTime.UtcNow });

        }

        private byte[] MakeRandomSecret()
        {
            //make secret for this email transaction
            byte[] secret = new byte[200];
            for (int i = 0; i < 200; i++)
                secret[i] = (byte)(ServerMain.Instance.myRandom.Next(256));

            return secret;
        }

        public List<SQLData> GetUserData(string userID, TimeSpan time)
        {
            string ip = GetIP();
            if (CheckDDOS(ip)) return null;

            if (time == null || time.TotalDays > 30 || time.Ticks <= 0)
                return null;

            Supporting.WriteLog($"[{ip}]:Served userdata [{userID.Substring(0, 8)}][{time}]");
            ServerMain.Instance.myStats.Add(new ServerMain.ServerStats { ip = ip, served = ServerMain.Served.GetLocal, time = DateTime.UtcNow });

            return ServerMain.Instance.mySQL.GetDataListUser(userID, time);
        }

        public List<SQLData> GetTimeZoneData(int timeZone, TimeSpan time)
        {
            string ip = GetIP();
            if (CheckDDOS(ip)) return null;

            if (timeZone < -12 || timeZone > 12 || time == null || time.TotalDays > 30 || time.Ticks <= 0)
                return null;

            List<SQLData> result = new List<SQLData>();

            List<SQLData> rawData = ServerMain.Instance.mySQL.GetDataListTimeZone(timeZone, time);

            if (rawData.Count == 0) return null;

            for (int i = -Convert.ToInt32(time.TotalMinutes); i <= 0; i += 5)
            {
                DateTime start = DateTime.UtcNow + TimeSpan.FromMinutes(i);
                DateTime end = DateTime.UtcNow + TimeSpan.FromMinutes(i + 5);

                double meanS = 0, medianS = 0, stdDevS = 0;
                long count = 0;
                foreach(SQLData data in rawData)
                {
                    if (data.Time < start || data.Time > end)
                        continue;

                    meanS += data.Mean;
                    medianS += data.Median;
                    stdDevS += data.StdDev;
                    count++;
                }

                if (meanS == 0 || medianS == 0) continue;

                SQLData current = new SQLData();
                current.Mean = meanS / count;
                current.Median = medianS / count;
                current.StdDev = stdDevS / count;
                current.TimeZone = timeZone;
                current.Time = end;

                result.Add(current);
            }

            if (result.Count == 0 || result.Exists(p => p.Mean == double.NaN || p.Median == double.NaN || p.StdDev == double.NaN)) return null;

            Supporting.WriteLog($"[{ip}]:Served global data [{timeZone}][{time}]");
            ServerMain.Instance.myStats.Add(new ServerMain.ServerStats { ip = ip, served = ServerMain.Served.GetGlobal, time = DateTime.UtcNow });

            return result;
        }

        public string StatusReport()
        {
            string result = "";

            for (int i = 0; i < 5; i++)
                result += $"{(Served)i}:{ServerMain.Instance.myStats.Count(p => p.served == (Served)i)}|"; ;

            result += $"ip:{(from x in ServerMain.Instance.myStats select x.ip).Distinct().Count()}";

            return result;
        }
    }
}
