using Newtonsoft.Json;
using NHibernate;
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
    public class CollectingForecastWeatherService
    {
        TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
        readonly string OpenWeatherMapAppId;
        readonly TimeSpan RefreshRate;
        DateTime NextRetriveTime = DateTime.MinValue;
        DateTime NextRetriveForecastDate = DateTime.MinValue;
        readonly PeiuPlatform.DataAccessor.MysqlDataAccessor dataAccessor;
        public CollectingForecastWeatherService(string OpenWeatherAppId, 
            PeiuPlatform.DataAccessor.MysqlDataAccessor dataAccessor)
        {
            OpenWeatherMapAppId = OpenWeatherAppId;
            this.dataAccessor = dataAccessor;
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

        private async Task SaveOrUpdate(IEnumerable<ForecastWeather> datas, IStatelessSession session)
        {
            
            foreach (var weather in datas)
                await session.InsertAsync(weather);
        }

        public async Task UploadingForecastWeather(CancellationToken stoppingToken)
        {
            try
            {
                Logging("Starting collects forecast weather...");

                using (var session = dataAccessor.SessionFactory.OpenStatelessSession())
                using (var Transaction = session.BeginTransaction())
                {
                    Logging("Truncate forecast weather on before works");
                    await Clear(session, stoppingToken);
                    Logging("Working to collect forecast weather...(5 days / 3 hours)");
                    var allOfSites = await session.CreateCriteria<Vwcontractorsite>().ListAsync<Vwcontractorsite>();
                    List<ForecastWeather> results = new List<ForecastWeather>();
                    foreach (Vwcontractorsite site in allOfSites)
                    {
                        Logging($"Request Forecast weather at {site.Siteid} ({site.Lng} / {site.Lat})");
                        ResponseForecast weather = await RequestForecastWeather(site.Lng, site.Lat);
                        if (weather == null)
                        {
                            Logging($"Failed to request forecast weather");
                            continue;
                        }
                        IEnumerable<ForecastWeather> currentweathers = ConvertForecastWether(site.Siteid, weather);
                        await SaveOrUpdate(currentweathers, session);

                        stoppingToken.ThrowIfCancellationRequested();
                        if (stoppingToken.IsCancellationRequested)
                            break;
                    }
                    await Transaction.CommitAsync();
                }

            }
            catch (Exception ex)
            {
                Logging(ex);
            }
            finally
            {
                //NextRetriveTime = DateTime.Now.Add(RefreshRate);
                Logging("Completed works.");
            }
        }
        
        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while(true)
        //    {
        //        stoppingToken.ThrowIfCancellationRequested();
        //        if (stoppingToken.IsCancellationRequested)
        //            break;
        //        if(DateTime.Now > NextRetriveForecastDate)
        //        {
        //            await UploadingForecastWeather(stoppingToken);
        //            NextRetriveForecastDate = DateTime.Now.Date.AddDays(1);
        //        }
        //        if (DateTime.Now > NextRetriveTime)
        //        {
        //            await UploadingCurrentWeather(stoppingToken);
        //            NextRetriveTime = DateTime.Now.Add(RefreshRate);
        //        }
        //        await Task.Delay(10);
        //    }
        //}

        //private async void SaveRedis(List<Currentweather> results)
        //{
        //    foreach(Currentweather weather in results)
        //    {
        //        string redisKey = "weather.sid" + weather.Siteid;
        //        HashEntry[] hashEntries = ConvertHashEntry<Currentweather>(weather, "ID");
        //        await redisDb.HashSetAsync(redisKey, hashEntries);
        //    }
        //}

        //private HashEntry[] ConvertHashEntry<T>(T src, params string[] MissingFields)
        //{
        //    List<HashEntry> hashEntries = new List<HashEntry>();
        //    foreach(var pi in  typeof(T).GetProperties())
        //    {
        //        if (MissingFields.Contains(pi.Name))
        //            continue;
        //        object value = pi.GetValue(src, null);
        //        if (value != null)
        //        {
        //            HashEntry hashEntry = new HashEntry(pi.Name, value.ToString());
        //            hashEntries.Add(hashEntry);
        //        }
        //    }
        //    return hashEntries.ToArray();
        //}

        private async Task Clear(IStatelessSession session, CancellationToken token)
        {
            var results = await session.CreateCriteria<ForecastWeather>().ListAsync<ForecastWeather>();
            int cnt = 0;
            foreach (ForecastWeather responseWeather in results)
            {
                await session.Query<ForecastWeather>().DeleteAsync(token);
                cnt++;
            }
            Logging($"All removed exist forecast weather ({cnt})");
        }

        //private async void SaveDbAsync<T>(List<T> results, CancellationToken token)
        //{
        //    using(var session = dbContext.SessionFactory.OpenStatelessSession())
        //    using(var transaction = session.BeginTransaction())
        //    {
                

        //        foreach (T responseWeather in results)
        //        {
        //            await session.InsertAsync(responseWeather, token);
        //        }
        //        await transaction.CommitAsync(token);
        //    }
            
        //}

        private IEnumerable<ForecastWeather> ConvertForecastWether(int siteId, ResponseForecast responseForecast)
        {
            foreach(ResponseBase responseWeather in responseForecast.list)
            {
                ForecastWeather currentweather = new ForecastWeather();
                currentweather.Clouds = responseWeather.clouds?.all;

                if (responseWeather.weather.Count > 0)
                {
                    Weather weather = responseWeather.weather[0];
                    currentweather.Code = weather.id;
                    currentweather.Description = weather.description;
                    currentweather.Main = weather.main;

                }
                if(responseWeather.wind != null)
                {
                    currentweather.Winddeg = responseWeather.wind.deg;
                    currentweather.Windspeed = responseWeather.wind.speed;
                }
                if (responseWeather.rain != null)
                {
                    currentweather.Rain1h = responseWeather.rain.Rain1h;
                    currentweather.Rain3h = responseWeather.rain.Rain3h;
                }
                if (responseWeather.snow != null)
                {
                    currentweather.Snow1h = responseWeather.snow.Snow1h;
                    currentweather.Snow3h = responseWeather.snow.Snow3h;
                }

                currentweather.Temperature = responseWeather.main.temp - 273.15f; //섭씨계산
                currentweather.Lowtemperature = responseWeather.main.temp_min - 273.15f;
                currentweather.Hightemperature = responseWeather.main.temp_max - 273.15f;
                currentweather.Pressure = responseWeather.main.pressure;
                currentweather.Humidity = responseWeather.main.humidity;
                currentweather.Createts = ToDateTime(responseWeather.dt);
                currentweather.Siteid = siteId;
                yield return currentweather;
            }
        }
            

        private DateTime ToDateTime(int unixtime)
        {
            DateTime utcTime = new DateTime(1970, 1, 1).AddSeconds(unixtime);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZoneInfo);
            return localTime;
        }

        private async Task<ResponseForecast> RequestForecastWeather(double lat, double lon)
        {
            string apiResponse = "";
            string url = $"http://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={OpenWeatherMapAppId}&lang=kr";
            try
            {
                
                /*Calling API http://openweathermap.org/api */
                HttpWebRequest apiRequest =
                WebRequest.Create(url) as HttpWebRequest;

                
                using (HttpWebResponse response = apiRequest.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    apiResponse = await reader.ReadToEndAsync();
                    //Logging(apiResponse);
                }
                /*End*/

                /*http://json2csharp.com*/
                ResponseForecast rootObject = JsonConvert.DeserializeObject<ResponseForecast>(apiResponse);
                return rootObject;
            }
            catch (Exception ex)
            {
                Logging(ex);
                Logging("url: " + url);
                Logging("apiResponse: \n" + apiResponse);
                return null;
            }
        }
    }
}
