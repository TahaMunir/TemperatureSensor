using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace UtilityClass
{
    public class SensorRecord
    {
        [BsonId] //_id
        public string SensorId { get; set; }
        public List<string> Value { get; set; }
        public List<DateTime> CreatedAt { get; set; }


        public SensorRecord(string sensorId)
        {
            this.SensorId = sensorId;
        }
    }
}

