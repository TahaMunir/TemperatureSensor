using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Windows.Controls;
using UtilityClass;

namespace Wpf.CartesianChart.Basic_Bars
{
    public partial class BasicColumn : UserControl
    {
        private SensorRecord _tempRecord;
        private DateTime _startTime;
        private string _sensorId;
        public const int DisplayField = 20;
        public const string MongoDbName  = "Measurements";
        public const string MongoCollectionName  = "SensorReadings"; 


        public BasicColumn()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection();
        }

        private void RenderControl(DateTime startTime, string sensorId)
        {
            // Fetch Data from the MongoDB Database according to query
            _tempRecord = ReadRecord();

            // If relevant data has been found prepare it forLive Chart
            if (_tempRecord != null)
            {
                var name = _tempRecord.SensorId;
                var sTimeStamp = new string[DisplayField];
                var valuesDouble = new double[DisplayField];


                for (var i = 0; i < _tempRecord.CreatedAt.Count; i++)
                {
                    // Check if less than 20 values has been returned from the DataBase
                    if (_tempRecord.Value[i] == null || _tempRecord.CreatedAt[i] == null)
                        break;
                    else
                    {
                        valuesDouble[i] = Convert.ToDouble(_tempRecord.Value[i]);
                        sTimeStamp[i] = _tempRecord.CreatedAt[i].ToString();
                    }
                }
                // Datapreparation for the LiveChartDisplay
                var columnSeries = new ColumnSeries
                {
                    Title = name,
                    Values = new ChartValues<double>(valuesDouble)
                };

                if (SeriesCollection == null) return;
                Labels = sTimeStamp;
                SeriesCollection.Clear();
                SeriesCollection.Add(columnSeries);
                Formatter = value => value.ToString("N");
                DataContext = this;
            }
            else
            {
                if (SeriesCollection == null) return;
                SeriesCollection.Clear();
                DataContext = this;

            }



        }

        private SensorRecord ReadRecord()
        {
            var db = new MongoDbCrud(MongoDbName, MongoCollectionName);
            var retData = db.GetSensorData(_startTime, _sensorId, MongoCollectionName, MongoDbName);
            return retData;
        }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        private void LoadBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _startTime = FromDtp.Value.HasValue ? Convert.ToDateTime(FromDtp.Value.ToString()) : DateTime.Now;
            RenderControl(Convert.ToDateTime(_startTime), _sensorId);
        }

        private void TxtUnit_TextChanged(object sender, TextChangedEventArgs e)
        {
            _sensorId = TxtUnit.Text;
        }

    }
}
