using Random_Experiment_Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Random_Experiment_Server.WCF
{
    public class AuthenticatedToken
    {
        public string Token { get; set; }
        public string IP { get; set; }
        public DateTime Time { get; set; }
    }

    public class RandomServer : IRandomServer
    {
        public static string GetIP()
        {
            try
            {
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

            return false;
        }

        public string GetToken()
        {
            string ip = GetIP();
            if (CheckDDOS(ip)) return null;

            string sessionToken = Convert.ToBase64String(this.MakeRandomSecret());
            this.AddToken(new AuthenticatedToken() { IP = ip, Token = sessionToken, Time = DateTime.UtcNow });

            return sessionToken;
        }

        public string SubmitStatus(string token, SQLData data)
        {
            string ip = GetIP();
            if (CheckDDOS(ip)) return null;

            if (!ServerMain.Instance.Sessions.Exists(p => p.IP == ip && p.Token == token))
                return "error:no token";

            AuthenticatedToken current = ServerMain.Instance.Sessions.Find(p => p.Token == token);
            ServerMain.Instance.Sessions.RemoveAll(p => p.IP == ip || p.Token == token);

            if (DateTime.UtcNow.Ticks - current.Time.Ticks < 0 || DateTime.UtcNow - current.Time > new TimeSpan(0, 5, 0))
                return "error:green";

            if (DateTime.UtcNow.Ticks - data.Time.Ticks < 0 || DateTime.UtcNow - data.Time > new TimeSpan(0, 2, 0))
                return "error:green";

            if (ServerMain.Instance.mySQL.AddData(data).IndexOf("error") == -1)
                return "error:SQL failed";

            Supporting.WriteLog($"[{ip}][{data.User}]:Added data[{data.Mean.ToString("N4")}][{data.Median.ToString("N4")}][{data.StdDev.ToString("N4")}]");
            
            return "success";
        }

        public void AddToken(AuthenticatedToken token)
        {
            if (CheckDDOS(GetIP())) return;

            ServerMain.Instance.Sessions.Add(token);

            Supporting.WriteLog($"[{token.IP}]:Added token");
        }

        private byte[] MakeRandomSecret()
        {
            //make secret for this email transaction
            byte[] secret = new byte[200];
            for (int i = 0; i < 200; i++)
                secret[i] = (byte)(ServerMain.Instance.myRandom.Next(256));

            return secret;
        }
    }
}
