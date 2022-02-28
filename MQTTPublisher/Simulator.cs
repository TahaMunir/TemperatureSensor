using System;
using System.Threading;
using System.Threading.Tasks;
using UtilityClass;

namespace Producer
{
    public delegate Task Notify(SensorMeasurement temperatureValue);  // delegate
    class Simulator
    {
        public event Notify DataGenerated; // event

        public string SensorId { get; set; }
        public int Samplerate { get; set; }

        public Simulator(string id, int samplerate)
        {
            this.SensorId = id;
            this.Samplerate = samplerate;
        }

        public void DataGenerator()
        {
            var rnd = new Random();
            var temperatureValues = new SensorMeasurement(this.SensorId)
            {
                Value = rnd.Next(20, 50),
                TimeStamp = DateTime.UtcNow
            };
            DataGenerated?.Invoke(temperatureValues);


        }

        public void Read()
        {
            Thread.Sleep(Samplerate);
            DataGenerator();
        }

    }
}
