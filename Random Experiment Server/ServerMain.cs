using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Random_Experiment_Server
{
    public class ServerMain
    {
        public static List<Tuple<string, DateTime>> DDOSlist = new List<Tuple<string, DateTime>>();

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
