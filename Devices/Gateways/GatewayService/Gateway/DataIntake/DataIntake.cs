namespace Microsoft.ConnectTheDots.Gateway
{
    using System;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public delegate void DataArrivalEventHandler( string data );

    public abstract class DataIntakeAbstract : IDataIntake
    {

        protected ILogger _Logger;

        protected DataIntakeAbstract( ILogger logger )
        {
            _Logger = SafeLogger.FromLogger( logger );
        }

        public abstract bool Start( Func<string, int> enqueue );

        public abstract bool Stop( );

        public abstract bool SetEndpoint( SensorEndpoint endpoint = null );
    }
}
