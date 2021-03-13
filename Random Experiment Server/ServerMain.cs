using Random_Experiment_Server.DB;
using Random_Experiment_Server.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Random_Experiment_Server
{
    public class ServerMain
    {
        public enum Served { Token, Submit, GetLocal, GetGlobal, DDOS }
        public class ServerStats
        {
            public Served served { get; set; }
            public string ip { get; set; }
            public DateTime time { get; set; }
        }

        public static ServerMain Instance;
        public SQLQueries mySQL { get; set; }
        public static List<Tuple<string, DateTime>> DDOSlist = new List<Tuple<string, DateTime>>();
        public Random myRandom = new Random((int)DateTime.UtcNow.Ticks % int.MaxValue);
        public List<AuthenticatedToken> Sessions { get; set; }
        public List<ServerStats> myStats { get; set; }

        public ServerMain()
        {
            if (Instance == null)
                Instance = this;
            else
                return;

            this.Sessions = new List<AuthenticatedToken>();
            this.mySQL = new SQLQueries();
            this.myStats = new List<ServerStats>();
        }

        public static bool CheckDDOS(string ip)
        {
            DateTime cutOff = DateTime.UtcNow.AddSeconds(-5);
            DDOSlist.RemoveAll(p => (DateTime)p.Item2 < cutOff);

            if (DDOSlist.Count(p => (string)p.Item1 == ip) > 5)
                return true;

            DDOSlist.Add(new Tuple<string, DateTime>(ip, DateTime.UtcNow));

            return false;
        }
    }
}
