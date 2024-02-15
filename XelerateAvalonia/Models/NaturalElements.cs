using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XelerateAvalonia.Models
{
    public class NaturalElements
    {
        public string Name { get; set; }

        public UniqueId ID { get; set; }

        public string StandardDeviation { get; set; }

        public string ZeroSum { get; set; }

        public string IsChecked { get; set; }


        // Defining the Meta data for each dataset -> should be loaded from database
        public NaturalElements(string name, UniqueId id,string standardDeviation, string zeroSum , string isChecked)
        {
            Name = name;
            ID = id;
            StandardDeviation = standardDeviation;
            ZeroSum = zeroSum;
            IsChecked = isChecked;
        }
    }
}
