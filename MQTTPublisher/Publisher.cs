using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilityClass;

namespace Producer
{
    public class Publisher
    {
        private static bool keepRunning = true;
        private static IMqttClient _client;
        private static MqttFactory _mqttFactory;
        private static KeyValuePair<DateTime, double> _sensorDataLog;
        private static System.Timers.Timer _watchTimer;
        private const int FailureDelay = 5; // 5 seconds Failure Delay

        public static string BrokerIp { get; set; }

        public static string BrokerPort { get; set; }

        public static string Topic { get; set; }



        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                Publisher.keepRunning = false;
            };
           
            if (args is { Length: 3 })
            {
                BrokerIp = args[0];
                BrokerPort = args[1];
                Topic = args[2];

            }
            var temperatureSensor = new Simulator(id: "rtd100", samplerate: 5);
            _mqttFactory = new MqttFactory();
            _client = _mqttFactory.CreateMqttClient();


            // register with an event
            temperatureSensor.DataGenerated += SensorListener;

            _client.UseConnectedHandler(e =>
            {
                Console.WriteLine("Connected to the broker successfully");
            });

            _client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Disconnected from the broker");
            });

            SetWatchTimer();

            while (keepRunning)
            {
                temperatureSensor.Read();
            }
        }

        /// <summary>
        /// Notify about the sensor failure at console
        /// </summary>
        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {

            if (_sensorDataLog.Key == null || DateTime.Now.Subtract(_sensorDataLog.Key).Seconds >= FailureDelay)
            {
                Console.WriteLine("Last sensor reading was received more than {0} seconds ago", FailureDelay);
            }

        }

        private static void PublishMessage(IMqttClient client, SensorMeasurement temperatureMeasurement)
        {
            string messagePayload = JsonConvert.SerializeObject(temperatureMeasurement);

            var message = new MqttApplicationMessageBuilder()
                              .WithTopic(Topic)
                              .WithPayload(messagePayload)
                              .WithAtLeastOnceQoS()
                              .Build();

            if (client.IsConnected)
            {
                client.PublishAsync(message);
                Console.WriteLine("Sensor: " + temperatureMeasurement.SensorId + "  published new temperature value:" + temperatureMeasurement.Value + " at TimeStamp:" + temperatureMeasurement.TimeStamp);

            }
        }
        /// <summary>
        /// /////Listener Method
        /// </summary>
        /// <param name="measurement"></param>
        /// Calls the publish method and pass the temperature measurements
        private static async Task SensorListener(SensorMeasurement measurement)
        {
            _sensorDataLog = new KeyValuePair<DateTime, double>(measurement.TimeStamp, measurement.Value);

            if (!_client.IsConnected)
            {
                var options = new MqttClientOptionsBuilder()
                    .WithClientId(Guid.NewGuid().ToString())
                    .WithTcpServer("localhost", 1883)
                    .WithCleanSession()
                    .Build();
                await _client.ConnectAsync(options);
                PublishMessage(_client, measurement);
            }
            else
            {
                PublishMessage(_client, measurement);
            }
        }
        /// <summary>
        ///// Timer to detect the failure of sensor
        /// </summary>
        private static void SetWatchTimer()
        {
            // Create a timer and set a failure delay interval.
            _watchTimer = new System.Timers.Timer();
            _watchTimer.Interval = FailureDelay*1000;

            // Hook up the Elapsed event for the timer. 
            _watchTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            _watchTimer.AutoReset = true;

            // Start the timer
            _watchTimer.Enabled = true;
        }

    }


}



