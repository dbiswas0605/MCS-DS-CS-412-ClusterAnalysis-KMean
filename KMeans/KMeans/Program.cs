using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMeans
{
    class DataSet
    {
        private double _latitude;
        private double _longitude;
        private int _clusterID;
        private int _position;

        internal DataSet(double lat, double lon, int clusterid, int position)
        {
            _latitude = lat;
            _longitude = lon;
            _clusterID = clusterid;
            _position = position;
        }

        public double Latitude { get => _latitude; }
        public double Longitude { get => _longitude; }
        public int ClusterID { get => _clusterID; set => _clusterID=value ; }
        public int Position { get => _position; set => _position = value; }
    }
    class KMean
    {
        private List<DataSet> _dataset = new List<DataSet>();
        private List<DataSet> _centroids = new List<DataSet>();


        internal void LoadData(string fileName)
        {
            var lines = System.IO.File.ReadAllLines(fileName);
            int i = 0;
            foreach(var line in lines)
            {
                double lat = Convert.ToDouble(line.Split(',')[0]);
                double lon = Convert.ToDouble(line.Split(',')[1]);

                _dataset.Add(new DataSet(lat, lon, 0, i++));
            }
        }

        internal void InitializeRandomCenteroidForClusters(int clusterCount)
        {
            //Random ran = new Random();
            //for (int i = 0; i < clusterCount; i++)
            //{
            //    int index = ran.Next(_dataset.Count);
                
            //    _centroids.Add(new DataSet(_dataset[i].Latitude,_dataset[i].Longitude,i));
            //}

            _centroids.Add(new DataSet(_dataset[0].Latitude, _dataset[0].Longitude, 0, 0));
            _centroids.Add(new DataSet(_dataset[4].Latitude, _dataset[4].Longitude, 1, 0));
            _centroids.Add(new DataSet(_dataset[6].Latitude, _dataset[6].Longitude, 2, 0));
        }
        
        internal void ReAdjustCentroid()
        {
            int[] clusterids = _dataset.Select(x => x.ClusterID).Distinct().ToArray();

            _centroids.Clear();
            foreach (int clusterid in clusterids)
            {
                var cluster = _dataset.Where(x => x.ClusterID == clusterid);

                double lat_mean = cluster.Sum(x => x.Latitude) / cluster.Count();
                double lon_mean = cluster.Sum(x => x.Longitude) / cluster.Count();

                _centroids.Add(new DataSet(lat_mean,lon_mean, clusterid, 0));
            }
        }

        internal bool ReAssignPointsToCluster()
        {
            bool clusterChanged = false;
            foreach(DataSet _ds in _dataset)
            {
                int nearestCluster = GetClosestCentroid(_ds);

                if(_ds.ClusterID != nearestCluster)
                {
                    _ds.ClusterID = nearestCluster;
                    clusterChanged = true;
                }
            }
            return clusterChanged;
        }

        private int GetClosestCentroid(DataSet datapoint)
        {
            List<Tuple<int, double>> distances = new List<Tuple<int, double>>();

            foreach (DataSet centeroid in _centroids)
            {
                distances.Add(Tuple.Create( centeroid.ClusterID, 
                    Math.Sqrt((datapoint.Latitude - centeroid.Latitude) * (datapoint.Latitude - centeroid.Latitude) +
                    (datapoint.Longitude - centeroid.Longitude) * (datapoint.Longitude - centeroid.Longitude))));
            }

            var closesetClusterID = distances.OrderBy(x => x.Item2).First().Item1;

            return closesetClusterID;

        }

        internal void MakeCluster()
        {
            bool keepOptimizing = true;

            while(keepOptimizing)
            {
                keepOptimizing = ReAssignPointsToCluster();
                ReAdjustCentroid();
            }
        }

        internal void Print()
        {
            var grp = _dataset.GroupBy(x => x.ClusterID);

            foreach(var g in grp)
            {
                Console.WriteLine(g.Key);

                foreach(var l in g.ToList())
                {
                    Console.WriteLine($"{l.Latitude}, {l.Longitude}");
                }
            }
        }

        internal void Write()
        {
            List<string> output = new List<string>();

            foreach(DataSet ds in _dataset)
            {
                output.Add($"{ds.Position} {ds.ClusterID}");
            }

            if (System.IO.File.Exists("clusters.txt"))
                System.IO.File.Delete("clusters.txt");

            System.IO.File.WriteAllLines("clusters.txt", output);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            KMean km = new KMean();

            string filename = "location.txt";
            //string filename = "location_demo.txt";


            km.LoadData(filename);
            km.InitializeRandomCenteroidForClusters(3);

            km.MakeCluster();

            km.Print();
            km.Write();
        }
    }
}
