using Newtonsoft.Json;
using NHibernate.Linq;
using PeiuPlatform.Models;
using PeiuPlatform.Models.Mysql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class CollectingCurrentWeatherService
    {
        TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
        readonly string OpenWeatherMapAppId;
        readonly TimeSpan RefreshRate;
        DateTime NextRetriveTime = DateTime.MinValue;
        DateTime NextRetriveForecastDate = DateTime.MinValue;
        readonly (double Lat, double Lon) ControlCenterLocation;
        readonly PeiuPlatform.DataAccessor.MysqlDataAccessor dataAccessor;
        public CollectingCurrentWeatherService(string OpenWeatherAppId, double ControlCenterGpsLat, double ControlCenterGpsLon,
            PeiuPlatform.DataAccessor.MysqlDataAccessor dataAccessor)
        {
            OpenWeatherMapAppId = OpenWeatherAppId;
            ControlCenterLocation = (Lat: ControlCenterGpsLat, Lon: ControlCenterGpsLon);
            this.dataAccessor = dataAccessor;
        }

        public async Task UploadingCurrentWeather(CancellationToken stoppingToken)
        {
            try
            {
                using(var session = dataAccessor.SessionFactory.OpenSession())
                using(var Transaction = session.BeginTransaction())
                {
                    var allOfSites = await session.CreateCriteria<Vwcontractorsite>().ListAsync<Vwcontractorsite>();

                    foreach (var site in allOfSites)
                    {
                        ResponseWeather weather = await RequestWeatherInformation(site.Lng, site.Lat);
                        if (weather == null)
                            continue;
                        CurrentWeather currentweather = ConvertWeather(site.Siteid, weather);
                        await session.SaveOrUpdateAsync(currentweather);

                        stoppingToken.ThrowIfCancellationRequested();
                        if (stoppingToken.IsCancellationRequested)
                            break;
                    }

                    ResponseWeather cc_weather = await RequestWeatherInformation(ControlCenterLocation.Lat, ControlCenterLocation.Lon);
                    if (cc_weather == null)
                        return;
                    CurrentWeather cc_currentweather = ConvertWeather(0, cc_weather);
                    await session.SaveOrUpdateAsync(cc_currentweather);
                    await Transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                Logging(ex);
            }
            finally
            {
                Logging("Completed");
            }
        }

        private void Logging(Exception ex)
        {
            Logging(ex.Message +"\t" + ex.StackTrace);
        }

        private void Logging(string message)
        {
            string dt = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
            Console.WriteLine($"[{dt}] {message}");
        }

        private CurrentWeather ConvertWeather(int siteId, ResponseWeather responseWeather)
        {
            CurrentWeather currentweather = new CurrentWeather();
            currentweather.Clouds = responseWeather.clouds?.all;

            if (responseWeather.weather.Count > 0)
            {
                Weather weather = responseWeather.weather[0];
                currentweather.Code = weather.id;
                currentweather.Description = weather.description;
                currentweather.Main = weather.main;
                currentweather.Icon = weather.icon;

            }
            if(responseWeather.rain != null)
            {
                currentweather.Rain1h = responseWeather.rain.Rain1h;
                currentweather.Rain3h = responseWeather.rain.Rain3h;
            }
            if(responseWeather.snow != null)
            {
                currentweather.Snow1h = responseWeather.snow.Snow1h;
                currentweather.Snow3h = responseWeather.snow.Snow3h;
            }

            if(responseWeather.wind != null)
            {
                currentweather.Winddeg = responseWeather.wind.deg;
                currentweather.Windspeed = responseWeather.wind.speed;
            }

            currentweather.Temperature = responseWeather.main.temp - 273.15f; //섭씨계산
            currentweather.Lowtemperature = responseWeather.main.temp_min - 273.15f;
            currentweather.Hightemperature = responseWeather.main.temp_max - 273.15f;
            currentweather.Pressure = responseWeather.main.pressure;
            currentweather.Humidity = responseWeather.main.humidity;
            currentweather.Sunrise = ToDateTime(responseWeather.sys.sunrise);
            currentweather.Sunset = ToDateTime(responseWeather.sys.sunset);
            currentweather.Createts = ToDateTime(responseWeather.dt);
            currentweather.Siteid = siteId;
            currentweather.Cityname = responseWeather.name;
            currentweather.Lat = responseWeather.coord.lat;
            currentweather.Lng = responseWeather.coord.lon;
            return currentweather;
        }

        private DateTime ToDateTime(int unixtime)
        {
            DateTime utcTime = new DateTime(1970, 1, 1).AddSeconds(unixtime);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZoneInfo);
            return localTime;
        }

        private async Task<ResponseWeather> RequestWeatherInformation(double lat, double lon)
        {
            string apiResponse = "";
            string url = "";
            try
            {
                /*Calling API http://openweathermap.org/api */
                url = $"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={OpenWeatherMapAppId}&lang=kr";
                HttpWebRequest apiRequest =
                WebRequest.Create(url) as HttpWebRequest;


                using (HttpWebResponse response = apiRequest.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    apiResponse = await reader.ReadToEndAsync();
                }
                /*End*/

                /*http://json2csharp.com*/
                ResponseWeather rootObject = JsonConvert.DeserializeObject<ResponseWeather>(apiResponse);
                return rootObject;
            }
            catch (Exception ex)
            {

                Logging($"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}\nRequest Url: {url}\nResponse: {apiResponse}");
                return null;
            }
        }
    }
}
