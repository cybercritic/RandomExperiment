using Random_Experiment_Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace Random_Experiment_Server.WCF
{
    [ServiceContract(Namespace = "")]
    interface IRandomServer
    {
        [OperationContract]
        [WebGet(UriTemplate = "token", BodyStyle = WebMessageBodyStyle.Bare)]
        string GetToken();

        [OperationContract]
        [WebInvoke(UriTemplate = "sub?t={token}", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string SubmitStatus(string token, SQLData data);

        [OperationContract]
        [WebInvoke(UriTemplate = "get?u={userID}", BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<SQLData> GetUserData(string userID, TimeSpan time);

        [OperationContract]
        [WebInvoke(UriTemplate = "zone?z={timezone}", BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<SQLData> GetTimeZoneData(int timeZone, TimeSpan time);

        [OperationContract]
        [WebGet(UriTemplate = "status_report",BodyStyle =WebMessageBodyStyle.Bare)]
        string StatusReport();
    }
}
