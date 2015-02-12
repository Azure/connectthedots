using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using Gateway.Models;
using Gateway.ServiceInstantiation;

namespace Gateway
{
    [ServiceContract(Namespace = "GatewayService")]
    public interface IGatewayService : IService
    {
        [WebGet()]
        [OperationContract]
        int Enqueue(string jsonData);
    }
}
