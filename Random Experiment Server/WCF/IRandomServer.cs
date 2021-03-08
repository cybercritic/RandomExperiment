using Random_Experiment_Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Random_Experiment_Server.WCF
{
    [ServiceContract(Namespace = "")]
    interface IRandomServer
    {
        [OperationContract]
        string GetToken();

        [OperationContract]
        string SubmitStatus(string token, SQLData data);

        [OperationContract]
        List<SQLData> GetUserData(string userID, int days);

        [OperationContract]
        List<SQLData> GetTimeZoneData(int timeZone, int days);
    }
}
