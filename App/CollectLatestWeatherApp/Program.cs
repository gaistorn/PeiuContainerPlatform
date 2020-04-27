using PeiuPlatform.DataAccessor;
using PeiuPlatform.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    class Program
    {
        const string OPENWEATHER_APP_KEY = "ENV_OPENWEATHER_APP_KEY";
        const string CC_GPS_LAT = "ENV_CC_GPS_LAT";
        const string CC_GPS_LON = "ENV_CC_GPS_LON";
        
        static void Main(string[] args)
        {
            Console.WriteLine("Mining weather data!");
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

//#if DEBUG
//            Console.WriteLine("DEBUG MODE");
//            string openWeatherAppKey = "0e24126ab1639fb0301e58fb0f2a7009";
//            string controlCenterLat = "36.429409";
//            string controlCenterLon = "127.390811";
//            string mysql_conn = DataAccessor.CreateConnectionString("192.168.0.40", "3306", "peiugrid", "power21", "123qwe");
//            MysqlDataAccessor dataAccessor = new MysqlDataAccessor(mysql_conn);
//#else
            string openWeatherAppKey = Environment.GetEnvironmentVariable(OPENWEATHER_APP_KEY);
            string controlCenterLat = Environment.GetEnvironmentVariable(CC_GPS_LAT);
            string controlCenterLon = Environment.GetEnvironmentVariable(CC_GPS_LON);
            MysqlDataAccessor dataAccessor = MysqlDataAccessor.CreateDataAccessFromEnvironment();
//#endif

            double lat = double.Parse(controlCenterLat);
            double lon = double.Parse(controlCenterLon);

            

            CollectingCurrentWeatherService svc = new CollectingCurrentWeatherService(openWeatherAppKey, lat, lon, dataAccessor);

            Task t = svc.UploadingCurrentWeather(cancellationTokenSource.Token);
            t.Wait();
        }
    }
}
