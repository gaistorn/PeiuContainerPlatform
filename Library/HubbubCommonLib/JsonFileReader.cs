using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PeiuPlatform.Hubbub
{
    public static class JsonFileReader
    {
        
        public static IEnumerable<T> OpenFileInFolder<T>(string FolderLocation)
        {
            string[] files = Directory.GetFiles(FolderLocation, "*.json");
            List<T> result = new List<T>();
            foreach (string file in files)
            {
                try
                {
                    T model = JsonConvert.DeserializeObject<T>(File.ReadAllText(file, Encoding.UTF8));
                    result.Add(model);
                }
                finally { }
            }
            return result;
        }
    }
}
