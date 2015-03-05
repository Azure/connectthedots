namespace Microsoft.ConnectTheDots.Gateway
{
    using System.ServiceModel;
    using System.ServiceModel.Web;

    //--//

    [ServiceContract( Namespace = "GatewayService" )]
    public interface IGatewayService : IService
    {
        [WebGet( )]
        [OperationContract]
        int Enqueue( string jsonData );
    }
}
