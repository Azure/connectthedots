namespace Microsoft.ConnectTheDots.Gateway
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    //--//

    class ServiceInstanceProvider : IInstanceProvider
    {
        private readonly Func<IService> _serviceCreator;

        //--//

        public ServiceInstanceProvider( Func<IService> serviceCreator )
        {
            this._serviceCreator = serviceCreator;
        }

        public object GetInstance( InstanceContext instanceContext, Message message )
        {
            return _serviceCreator( );
        }

        public object GetInstance( InstanceContext instanceContext )
        {
            return _serviceCreator( );
        }

        public void ReleaseInstance( InstanceContext instanceContext, object instance )
        {
        }
    }
}
