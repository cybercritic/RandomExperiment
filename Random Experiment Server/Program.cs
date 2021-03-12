using Random_Experiment_Server.WCF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Random_Experiment_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerMain serverMain = new ServerMain();

            Uri baseAddress = new Uri("http://127.0.0.1:3030");

            ServiceHost serviceHost = new ServiceHost(typeof(RandomServer), baseAddress);

            WSHttpBinding binding = new WSHttpBinding();
            binding.OpenTimeout = new TimeSpan(0, 5, 0);
            binding.CloseTimeout = new TimeSpan(0, 5, 0);
            binding.SendTimeout = new TimeSpan(0, 10, 0);
            binding.ReceiveTimeout = new TimeSpan(long.MaxValue);
            binding.MaxBufferPoolSize = 1024 * 1024 * 10;
            binding.MaxReceivedMessageSize = 1024 * 1024;
            binding.ReaderQuotas.MaxArrayLength = 1000000;
            binding.ReaderQuotas.MaxStringContentLength = 1000000;
            
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            smb.HttpGetEnabled = true;
            serviceHost.Description.Behaviors.Add(smb);

            serviceHost.AddServiceEndpoint(typeof(IRandomServer), binding, baseAddress);
            serviceHost.CloseTimeout = new TimeSpan(long.MaxValue);
            serviceHost.OpenTimeout = new TimeSpan(long.MaxValue);
            
            serviceHost.Open();

            Console.WriteLine("The service is ready at {0}", baseAddress);
            Console.WriteLine("Press <Enter> to stop the service.");
            Console.ReadLine();

            // Close the ServiceHost.
            serviceHost.Close();
        }
    }

    public class Supporting
    {
        private static Object thisLock = new Object();

        public const string LogDir = @"\LOG";
        public const string LogFile = @"\logfile.txt";
        //Directory.GetCurrentDirectory() + @"\DB\plutus_test_db.mdf";
        public static void WriteLog(string content)
        {
            string curDir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(curDir + LogDir))
                Directory.CreateDirectory(curDir + LogDir);

            lock (thisLock)
            {
                File.AppendAllText(curDir + LogDir + LogFile, DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss ->") + content + "\r\n");
            }

            content = content.TrimEnd();
            Console.WriteLine(DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss ->") + content);
        }
    }
}
