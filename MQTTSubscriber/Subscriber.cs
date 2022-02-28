using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilityClass;

namespace Consumer
{
    public class Subscriber
    {
        private static bool keepRunning = true;
        private static IMqttClient _client;
        private static MqttFactory _mqttFactory;

        public static string BrokerIp { get; set; }

        public static string BrokerPort { get; set; }

        public static string Topic { get; set; }

        public static string DbName { get; set; }

        public static string CollectionName { get; set; }

        static async Task Main(string[] args)
        {

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                Subscriber.keepRunning = false;
            };

            if (args is {Length: 5})
            {
                BrokerIp   = args[0]; 
                BrokerPort = args[1];
                Topic = args[2];
                DbName = args[3];
                CollectionName = args[4];

            }


            _mqttFactory = new MqttFactory();
            _client = _mqttFactory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(BrokerIp, Convert.ToInt32(BrokerPort))
                .WithCleanSession()
                .Build();
            /////////////////////////////////////////////////////////////
            // As soon as client is connected to broker subscribe to topic
            //////////////////////////////////////////////////////////////
            _client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Connected to the broker successfully");
                var topicFilter = new MqttTopicFilterBuilder()
                                     .WithTopic(Topic)
                                     .Build();
        
                await _client.SubscribeAsync(topicFilter);


            });

            _client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Disconnected from the broker");
            });

            //////////////////////////////////////////////////////////
            // As soon as new message arrives update the Database
            //////////////////////////////////////////////////////////
            _client.UseApplicationMessageReceivedHandler(async e =>
            {
                var payload = e.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(e.ApplicationMessage?.Payload);
                var sensor = JsonConvert.DeserializeObject<SensorMeasurement>(payload);
                if (sensor != null)
                {
                    Console.WriteLine("New temperature value: " + sensor.Value + " received from sensor: " + sensor.SensorId + " with TimeStamp + "+ sensor.TimeStamp);
                    await AddRecord(sensor);
                }
            });


            while (keepRunning)
            {
                if (!_client.IsConnected)
                {
                    await _client.ConnectAsync(options);
                }

            }
        }

        public static async Task AddRecord(SensorMeasurement measurement)
        {
            var db = new MongoDbCrud(DbName, CollectionName);
            await db.InsertRecord(measurement);
        }
    }
}
