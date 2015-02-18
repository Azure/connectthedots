using System;
using System.Threading;
using System.Threading.Tasks;
using Gateway.Utils.Logger;
using Gateway.Models;
using System.Collections.Generic;

namespace Gateway.DataIntake
{
    public delegate void DataArrivalEventHandler( string data );

    public abstract class DataIntakeAbstract : IDataIntake
    {

        protected ILogger _Logger;

        protected DataIntakeAbstract( ILogger logger )
        {
            _Logger = new SafeLogger( logger );
        }

        public abstract bool Start( Func<string, int> enqueue );

        public abstract bool Stop( );

        public abstract bool SetEndpoint( SensorEndpoint endpoint );

        public void SignalDataArrival( string data )
        {
            DataArrivalEventHandler onData = OnDataArrival;

            if(onData != null)
            {
                onData( data );
            }
        }

        public event DataArrivalEventHandler OnDataArrival;
    }
}
