namespace Microsoft.ConnectTheDots.GatewayService
{
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.ServiceProcess;
    using Microsoft.ConnectTheDots.Gateway;

    //--//

    [RunInstaller( true )]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller _process;
        private ServiceInstaller        _service;

        //--//

        public ProjectInstaller( )
        {
            _process = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };
            _service = new ServiceInstaller
            {
                ServiceName = Constants.WindowsServiceName
            };
            Installers.Add( _process );
            Installers.Add( _service );
        }
    }
}