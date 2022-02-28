using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using GalaSoft.MvvmLight;
using UtilityClass;
using LiveCharts;
using LiveCharts.Wpf;

namespace WpfApp.Object.Lib.ViewModel
{
    public class DisplayViewModel : ViewModelBase
    {
        private SensorRecord _tempRecord;
        public double valueTemp;

        public DisplayViewModel()
        {
            ;
        }

        public void renderControl()
        {
            _tempRecord = readRecordMongo();
            string name;
            List<string> values;
            List<DateTime> timeStamp;
            values = new List<string>(new string[20]);
            timeStamp = new List<DateTime>(new DateTime[20]);

            if (_tempRecord == null)
            {
                name = "";
                values = new List<string> { };
                timeStamp = new List<DateTime> { };
            }
            else
            {
                name = _tempRecord.SensorId;

                for (int i = 0; i < 20; i++)
                {
                    values[i] = _tempRecord.Value[i];
                    timeStamp[i] = _tempRecord.CreatedAt[i];
                }

            }

            List<double> valuesDouble = values.ConvertAll(item => double.Parse(item));
            string[] stringStamp = new string[timeStamp.Count];
            for (int i = 0; i < timeStamp.Count; i++)
            {
                stringStamp[i] = timeStamp[i].ToString();
            }
            SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = name,
                        Values = new ChartValues<double>(valuesDouble)
                    }
                };
           // Labels = stringStamp;
           // Formatter = value => value.ToString("N");
        }

        public SensorRecord readRecordMongo()
        {
            MongoCRUD db = new MongoCRUD("Measurements", "SensorReadings");
            SensorRecord retData = db.ReadRecord();
            return retData;
        }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
    }
}
