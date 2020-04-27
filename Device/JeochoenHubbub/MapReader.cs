using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PeiuPlatform.Hubbub
{
    public class MapReader : IDisposable
    {
        readonly StreamReader reader;
        public MapReader(string fileName)
        {
            reader = new StreamReader(fileName, Encoding.UTF8);
            
        }

        public IEnumerable<DataPoint> ReadToEnd()
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            reader.ReadLine(); // read a header
            List<DataPoint> points = new List<DataPoint>();
            int readLine = 1;
            while (reader.EndOfStream == false)
            {
                string line = reader.ReadLine();
                string[] splits = line.Split(',');
                DataPoint point = new DataPoint();
                try
                {
                    point.Category = splits[0];
                    point.Address = ushort.Parse(splits[1]);
                    point.WordSize = int.Parse(splits[2]);
                    point.Name = splits[4];
                    point.Digit = int.Parse(splits[5]);
                    point.Description = splits[6];
                    point.Type = (DataType)Enum.Parse(typeof(DataType), splits[7]);
                    point.Scale = float.Parse(splits[8]);
                    points.Add(point);
                    readLine++;
                }
                catch(Exception ex)
                {
                    logger.Error(ex, $"[row:{readLine}] {ex.Message}");
                }
            }
            return points;
            
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
