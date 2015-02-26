using System;
using Gateway.Models;

namespace CoreTest.Utils.Generators
{
    public static class RandomSensorDataGenerator
    {
        //Simple generator for initial testing
        public static SensorDataContract Generate()
        {
            Random r = new Random();
            int rint = r.Next() % 2, cint = r.Next() % 2;
            SensorDataContract sensorData = new SensorDataContract
            {
                MeasureName = rint == 0 ? "length" : "time",
                UnitOfMeasure = rint == 0 ? "m" : "s",
                DisplayName = "Sensor" + cint + (rint == 0 ? "m" : "s"),
                Guid = new Guid().ToString(),
                Value = r.Next() % 1000 - 500,
                Location = "here",
                Organization = "contoso",
                TimeCreated = DateTime.UtcNow
            };
            return sensorData;
        }
    }
}
