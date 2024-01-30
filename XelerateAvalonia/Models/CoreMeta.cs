using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XelerateAvalonia.Models
{
    public class CoreMeta
    {
        public string Name { get; set; }

        public UniqueId ID {get; set;}

        public string DeviceUsed { get; set; }

        public string InputSource { get; set; }

        public float MeasuredTime { get; set; }

        public float Voltage { get; set; }

        public float Current { get; set; }

        public float Size { get; set; }

        public DateOnly Uploaded { get; set; }


        // Defining the Meta data for each dataset -> should be loaded from database
        public CoreMeta(string name, UniqueId id , string deviceUsed, string inputSource, float measuredTime, float voltage, float current, float size, DateOnly uploaded)
        {
            Name = name;
            ID = id;
            DeviceUsed = deviceUsed;
            InputSource = inputSource;
            MeasuredTime = measuredTime;
            Voltage = voltage;
            Current = current;
            Size = size;
            Uploaded = uploaded;
            
        }
    }
}
