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
        public static ServerMain Instance;
        public static List<Tuple<string, DateTime>> DDOSlist = new List<Tuple<string, DateTime>>();
        public Random myRandom = new Random((int)DateTime.UtcNow.Ticks % int.MaxValue);
        public List<AuthenticatedToken> Sessions { get; set; }

        public ServerMain()
        {
            if (Instance == null)
                Instance = this;
            else
                return;

            this.Sessions = new List<AuthenticatedToken>();
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
