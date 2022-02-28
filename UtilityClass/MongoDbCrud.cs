﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace UtilityClass
{

    public class MongoDbCrud
    {
        private static bool _collectionInitialized;
        public const int DisplayField = 20;
        private IMongoDatabase _db;
        private SensorRecord _mongoDbDocument;

        public MongoClient Client;


        public MongoDbCrud(string dbName, string collectionName)
        {
            DbName = dbName;
            CollectionName = collectionName;
            Client = new MongoClient();
            _db = Client.GetDatabase(DbName);
        }

        public string DbName { get; set; }
        public string CollectionName { get; set; }

        public async Task InsertRecord(SensorMeasurement measurement)
        {
            var tempRecord = new SensorRecord(measurement.SensorId);
            if (!_collectionInitialized)
            {
                var collection = _db.GetCollection<SensorRecord>(CollectionName);
                // Initialize record
                tempRecord.Value = new List<string> {Convert.ToString(measurement.Value)};
                tempRecord.CreatedAt = new List<DateTime> {measurement.TimeStamp};
                try
                {
                    await collection.InsertOneAsync(tempRecord);
                    _collectionInitialized = true;
                }
                catch
                {
                    _collectionInitialized = true;
                }
            }
            else
            {
                var collection = _db.GetCollection<SensorRecord>(CollectionName);
                var filter = Builders<SensorRecord>.Filter.Eq(a => a.SensorId, measurement.SensorId);
                var update = Builders<SensorRecord>.Update
                    .Push(t => t.Value, Convert.ToString(measurement.Value))
                    .Push(t => t.CreatedAt, measurement.TimeStamp);
                var addNewReading = await collection
                    .UpdateOneAsync(filter, update);
            }
        }

        public SensorRecord GetSensorData(DateTime startTime, string sensorId, string collectionName, string dbName)
        {
            _db = Client.GetDatabase(dbName);

            _mongoDbDocument = FetchData(sensorId, collectionName);
            if (_mongoDbDocument != null)
            {
                var index = FindStartIndex(startTime);
                if (index >= 0)
                {
                    var uidata = new SensorRecord(sensorId)
                    {
                        Value = new List<string>(new string[DisplayField]),
                        CreatedAt = new List<DateTime>(new DateTime[DisplayField])
                    };
                    for (var i = 0; i < DisplayField && i < (_mongoDbDocument.Value.Count - index); i++)
                    {
                        uidata.Value[i] = _mongoDbDocument.Value[index + i];
                        uidata.CreatedAt[i] = _mongoDbDocument.CreatedAt[index + i];
                    }

                    return uidata;
                }

                return null;
            }

            return null;
        }

        private SensorRecord FetchData(string sensorid, string collectionName)
        {
            var mongodbFilterBuilder = Builders<SensorRecord>.Filter;
            var mongodbFilter = mongodbFilterBuilder.Eq(r => r.SensorId, sensorid);
            var collection = _db.GetCollection<SensorRecord>(collectionName);
            var query = collection
                .AsQueryable()
                .Where(x => mongodbFilter.Inject());
            var retData = query.FirstOrDefault();
            return retData;
        }

        private int FindStartIndex(DateTime startTime)
        {
            var startIndex = -1;
            for (var i = 0; i < _mongoDbDocument.CreatedAt.Count; i++)
            {
                if (_mongoDbDocument.CreatedAt[i].Subtract(startTime).TotalSeconds < 0) continue;
                startIndex = i;
                break;
            }

            return startIndex;
        }
    }
}