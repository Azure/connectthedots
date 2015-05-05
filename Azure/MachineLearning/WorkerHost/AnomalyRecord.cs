using System;

namespace WorkerHost
{
    public class AnomalyRecord
    {
        public DateTime Time { get; set; }
        public double Data { get; set; }
        public int Spike1 { get; set; }
        public int Spike2 { get; set; }
        public double LevelScore { get; set; }
        public int LevelAlert { get; set; }
        public double TrendScore { get; set; }
        public int TrendAlert { get; set; }

        public static AnomalyRecord Parse(string[] values)
        {
            if (values.Length < 8)
                throw new ArgumentException("Anomaly Record expects 8 values.");
            return new AnomalyRecord()
            {

                Time = DateTime.Parse(values[0]),
                Data = double.Parse(values[1]),
                Spike1 = int.Parse(values[2]),
                Spike2 = int.Parse(values[3]),
                LevelScore = double.Parse(values[4]),
                LevelAlert = int.Parse(values[5]),
                TrendScore = double.Parse(values[6]),
                TrendAlert = int.Parse(values[7]),
            };
        }

        public override string ToString()
        {
            return Time.ToLocalTime() + ", " + Data + ", " + Spike1 + ", " + Spike2 + ", " +
                LevelScore + ", " + LevelAlert + ", " + TrendScore + ", " + TrendAlert;
        }
    }
}
