using System;

namespace Gateway.DataIntake
{
    public interface IDataIntake
    {
        bool Start( Func<string, int> enqueue );

        bool Stop( );

        //leave endpoint null for Data Intakes that don't expect any endpoints
        bool SetEndpoint( SensorEndpoint endpoint = null );
    }
}
