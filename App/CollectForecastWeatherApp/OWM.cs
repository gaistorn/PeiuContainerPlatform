using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace PeiuPlatform.App
{
    public class Coord
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Main
    {
        public float temp { get; set; }
        public float pressure { get; set; }
        public float humidity { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
    }

    public class Wind
    {
        public float speed { get; set; }
        public float deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public double message { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class Rain
    {
        [JsonProperty("1h")]
        public float Rain1h { get; set; }

        [JsonProperty("3h")]
        public float Rain3h { get; set; }
    }

    public class Snow
    {
        [JsonProperty("1h")]
        public float Snow1h { get; set; }

        [JsonProperty("3h")]
        public float Snow3h { get; set; }
    }

    public class ResponseForecast
    {
        public string cod { get; set; }
        public float message { get; set; }
        public int cnt { get; set; }

        public List<ResponseBase> list { get; set; }
    }

    public class ResponseBase
    {
        public int dt { get; set; }
        public Main main { get; set; }
        public List<Weather> weather { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public Sys sys { get; set; }

        public Rain rain { get; set; }
        public Snow snow { get; set; }

    }

    public class ResponseWeather : ResponseBase
    {
        public Coord coord { get; set; }

        public string @base { get; set; }

        public int visibility { get; set; }


        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

}
