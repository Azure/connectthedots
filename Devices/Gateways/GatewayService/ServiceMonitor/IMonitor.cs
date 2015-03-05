namespace ServiceMonitor
{

    //--//

    interface IMonitor
    {
        bool Lock( string monitoringTarget );

        void Monitor( );

        void QuitMonitor( );
    }
}
