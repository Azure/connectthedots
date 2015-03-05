namespace Microsoft.ConnectTheDots.Gateway
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    //--//

    class ServiceInstanceProvider : IInstanceProvider
    {
        readonly Func<IService> serviceCreator;
        public ServiceInstanceProvider(Func<IService> serviceCreator)
        {
            this.serviceCreator = serviceCreator;
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return serviceCreator();
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return serviceCreator();
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }
    }
}
