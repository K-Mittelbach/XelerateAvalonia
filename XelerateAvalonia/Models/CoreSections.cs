using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XelerateAvalonia.Models
{
    public class CoreSections
    {
        public string CoreName { get; set; }

        public string SectionName { get; set; }

        public UniqueId ID { get; set; }

        public string StartRow { get; set; }

        public string EndRow { get; set; }

        public string IsChecked { get; set; }

        public CoreSections(string coreName, string sectioName, UniqueId id, string startRow, string endRow, string isChecked)
        {
            CoreName = coreName;
            SectionName = sectioName;
            ID = id;
            StartRow = startRow;
            EndRow = endRow;
            IsChecked = isChecked;
        }
    }
}
