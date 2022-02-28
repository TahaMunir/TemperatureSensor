using System;

namespace UtilityClass
{
    public class SensorMeasurement
    {
        public string SensorId { get; set; }
        public double Value { get; set; }
        public DateTime TimeStamp { get; set; }

        public SensorMeasurement(string sensorId)
        {
            this.SensorId = sensorId;
        }

    }
}
