namespace Microsoft.ConnectTheDots.Test
{
    using System;
    using Microsoft.ConnectTheDots.Gateway;

    //--//

    public static class RandomSensorDataGenerator
    {
        //Simple generator for initial testing
        private const int DEVICE_COUNT = 4;

        private static readonly Random _Random = new Random( );
        private static readonly string[] _Guids = new string[ DEVICE_COUNT ];
        private static readonly string[] _MeasureName = new string[ DEVICE_COUNT ];
        private static readonly string[] _UnitOfMeasure = new string[ DEVICE_COUNT ];
        private static readonly string[] _DisplayName = new string[ DEVICE_COUNT ];

        static RandomSensorDataGenerator( )
        {
            for( int i = 0; i < DEVICE_COUNT; ++i )
            {
                int rint = i % 2;

                _Guids[ i ] = ( new Guid( _Random.Next( ), 0, 0, new byte[ 8 ] ) ).ToString( );
                _MeasureName[ i ] = rint == 0 ? "length" : "time";
                _UnitOfMeasure[ i ] = rint == 0 ? "m" : "s";
                _DisplayName[ i ] = "Sensor" + i + ( rint == 0 ? "m" : "s" );
            }
        }

        public static SensorDataContract Generate( )
        {
            int device = _Random.Next( ) % DEVICE_COUNT;

            SensorDataContract sensorData = new SensorDataContract
            {
                MeasureName = _MeasureName[ device ],
                UnitOfMeasure = _UnitOfMeasure[ device ],
                DisplayName = _DisplayName[ device ],
                Guid = _Guids[ device ],
                Value = _Random.Next( ) % 1000 - 500,
                Location = "here",
                Organization = "contoso",
                TimeCreated = DateTime.UtcNow
            };
            return sensorData;
        }
    }
}
