using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XelerateAvalonia.Models
{
    public class Cluster
    {
        public string Name { get; set; }

        public UniqueId ID { get; set; }

        public byte[] ClusterPlot { get; set; }

        public string ClusterType { get; set; }

        public int ClusterNumbers { get; set; }

        public int[] ClusterID { get; set; }

        public Cluster(string name, UniqueId id,byte[] clusterPlot, string clusterType, int clusterNumbers, int[]clusterID )
        {
            Name = name;
            ID = id;
            ClusterPlot = clusterPlot;
            ClusterType = clusterType;
            ClusterNumbers = clusterNumbers;
            ClusterID = clusterID;
            

        }
    }
}
