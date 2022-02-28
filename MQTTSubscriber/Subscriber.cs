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


            //var endApp = false;
            var mqttFactory = new MqttFactory();
            var client = mqttFactory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(BrokerIp, Convert.ToInt32(BrokerPort))
                .WithCleanSession()
                .Build();
            client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Connected to the broker successfully");
                var topicFilter = new MqttTopicFilterBuilder()
                                     .WithTopic(Topic)
                                     .Build();
                await client.SubscribeAsync(topicFilter);


            });

            client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Disconnected from the broker");
            });

            client.UseApplicationMessageReceivedHandler(async e =>
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
                if (!client.IsConnected)
                {
                    await client.ConnectAsync(options);
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
