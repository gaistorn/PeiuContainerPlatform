using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public interface IHTMLGenerator
    {
        string GenerateHtml(string fileName, object parameter);
    }
    public class HTMLGenerator : IHTMLGenerator
    {
        public string GenerateHtml(string fileName, object parameter)
        {
            var encodedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(parameter));
            string notify_mail = File.ReadAllText(fileName);
            foreach (string key in encodedData.Keys)
            {
                notify_mail = notify_mail.Replace($"~{key}~", encodedData[key]);
            }

            //File.WriteAllText("NewNotifyEmail.html", notify_mail);

            return notify_mail;
        }
    }
}
