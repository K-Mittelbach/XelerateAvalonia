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

        public byte[] BlobROI { get; set; }

        public string StartRow { get; set; }

        public string EndRow { get; set; }

        public string HasImage { get; set; }

        public string IsChecked { get; set; }


        public CoreSections(string coreName, string sectioName, UniqueId id, byte[] blobROI, string startRow, string endRow, string hasImage, string isChecked)
        {
            CoreName = coreName;
            SectionName = sectioName;
            ID = id;
            BlobROI = blobROI;
            StartRow = startRow;
            EndRow = endRow;
            HasImage = hasImage;
            IsChecked = isChecked;
        }
    }
}
