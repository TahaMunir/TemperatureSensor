using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace WpfApp.Object.Lib.Models
{
    public class Display
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = "Sensor ID must be provided")]
        public string SensorId { get; set; }

        public int SliderValue { get; set; }

        public int CommandId
        {
            get; set;
        }
    }
}
