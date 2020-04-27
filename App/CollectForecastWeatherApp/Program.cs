using PeiuPlatform.DataAccessor;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    class Program
    {
        const string OPENWEATHER_APP_KEY = "ENV_OPENWEATHER_APP_KEY";

        static void Main(string[] args)
        {
            Console.WriteLine("Mining forecast weather app!");
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
            MysqlDataAccessor dataAccessor = MysqlDataAccessor.CreateDataAccessFromEnvironment();
            //#endif

            CollectingForecastWeatherService svc = new CollectingForecastWeatherService(openWeatherAppKey, dataAccessor);

            Task t = svc.UploadingForecastWeather(cancellationTokenSource.Token);
            t.Wait();
        }
    }
}
