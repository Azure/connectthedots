namespace Microsoft.ConnectTheDots.Gateway
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    //--//

    public class ServiceBehavior : IServiceBehavior
    {
        readonly Func<IService> serviceCreator;
        public ServiceBehavior( Func<IService> serviceCreator )
        {
            this.serviceCreator = serviceCreator;
        }

        public void AddBindingParameters( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters )
        {
        }

        public void ApplyDispatchBehavior( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
        {
            foreach( ChannelDispatcher cd in serviceHostBase.ChannelDispatchers )
            {
                foreach( EndpointDispatcher ed in cd.Endpoints )
                {
                    ed.DispatchRuntime.InstanceProvider = new ServiceInstanceProvider( this.serviceCreator );
                }
            }
        }

        public void Validate( ServiceDescription serviceDescription, ServiceHostBase serviceHostBase )
        {
        }
    }
}
