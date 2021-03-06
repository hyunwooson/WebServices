﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ical.Net;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Data.SqlClient;

namespace WebServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class F1Controller : ControllerBase
    {
        public class WeatherInfo
        {
            public double dt;
            public double sunset;
            public string night = "";
            public object temp;
            public List<Weather> weather;
        }
        public class Weather
        {
            public string id;
            public string main;
            public string description;
            public string icon;
        }
        private readonly ILogger<F1Controller> _logger;

        public F1Controller(ILogger<F1Controller> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get(string shift)
        {
            try
            {
                if (!Int32.TryParse(shift, out int timeShift))
                {
                    timeShift = 0;
                }

                string circuit = "";
                string latitude = "";
                string longitude = "";

                string text = "";

                var webRequest = WebRequest.Create(@"http://www.formula1.com/calendar/Formula_1_Official_Calendar.ics");

                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    text = reader.ReadToEnd();
                }
                var calendar = Calendar.Load(text);

                var seasonYear = calendar.Events[0].Start.AsUtc.Year;

                string evntName = "";
                var evntTime = new DateTime();
                bool seasonEnd = true;

                for (int i = 0; i < calendar.Events.Count; i++)
                {
                    if (DateTime.UtcNow < calendar.Events[i].Start.AsUtc)
                    {
                        evntName = calendar.Events[i].Summary.Trim();
                        evntTime = calendar.Events[i].Start.AsUtc;
                        circuit = calendar.Events[i].Location.Trim();
                        seasonEnd = false;
                        break;
                    }
                }

                if (seasonEnd)
                {
                    return new JObject()
                    {
                        { "title", $"{seasonYear} FORMULA 1 WORLD CHAMPIONSHIP™" },
                        { "session", "ENDED" },
                        { "date", "---. -, ----" },
                        { "time", "--:--"},
                        { "circuit", "TBA"},
                        { "url", "http://210.2.41.217:8880/files/kbo/emblems?team=TBA" },
                        { "flag", "" },
                        { "weather", "_000" },
                        { "temp", "" }
                    }.ToString();
                }

                string title = evntName.Substring(0, evntName.IndexOf(DateTime.Now.Year.ToString()) - 1);

                string session = evntName.Substring(evntName.LastIndexOf('-') + 2);

                string date;
                if (evntTime.AddSeconds(timeShift).Date == DateTime.UtcNow.AddSeconds(timeShift).Date)
                    date = "Today";
                else if (evntTime.AddSeconds(timeShift).Date == DateTime.UtcNow.AddDays(1).AddSeconds(timeShift).Date)
                    date = "Tomorrow";
                else
                    date = evntTime.AddSeconds(timeShift).ToString("MMM. d, yyyy");

                string time = evntTime.AddSeconds(timeShift).ToString("HH:mm");



                string circuit_url = "";

                string flag_url = "";

                string connectionString = "Server=192.168.0.4;Database=WebServices;User Id=sa;Password=thsgusdn369;";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand($"SELECT LATITUDE, LONGITUDE, CIRCUIT_IMG_URL, FLAG_IMG_URL FROM TB_F1_RESOURCES WHERE LOCATION = '{circuit}'", con))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            latitude = reader.GetString(0);
                            longitude = reader.GetString(1);
                            circuit_url = reader.GetString(2);
                            flag_url = reader.GetString(3);
                        }
                    }
                }

                string owmUrl = $"https://api.openweathermap.org/data/2.5/onecall?lat={latitude}&lon={longitude}&appid=14946c8bd54652131af03989ad323e19&unit=metric";
                HttpWebRequest weatherRequest = (HttpWebRequest)WebRequest.Create(owmUrl);
                weatherRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                string resultTxt = "";

                using (HttpWebResponse response = (HttpWebResponse)weatherRequest.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    resultTxt = reader.ReadToEnd();
                }

                var result = JObject.Parse(resultTxt);

                string weatherCd = "000";
                double temp = 0;


                if (!result.TryGetValue("cod", out JToken jt))
                {
                    double timezone = result["timezone_offset"].Value<double>();

                    if (date.Equals("Today") || date.Equals("Tomorrow"))
                    {
                        var array = result["hourly"].Value<JArray>();
                        List<WeatherInfo> forecasts = array.ToObject<List<WeatherInfo>>();
                        foreach (var item in forecasts)
                        {
                            var weatime = UnixTimeStampToDateTime(item.dt, 0);
                            if (evntTime < weatime)
                            {
                                weatherCd = item.weather[0].id;
                                temp = Math.Round(double.Parse(item.temp.ToString()) - 273.15, 0);
                                break;
                            }
                        }
                        var array_d = result["daily"].Value<JArray>();
                        List<WeatherInfo> forecasts_d = array_d.ToObject<List<WeatherInfo>>();
                        foreach (var item in forecasts_d)
                        {
                            var weatime = UnixTimeStampToDateTime(item.dt, 0);
                            if (evntTime.AddSeconds(timezone).Date.Equals(weatime.AddSeconds(timezone).Date))
                            {
                                var sunsetTime = UnixTimeStampToDateTime(item.sunset, timezone);
                                if (evntTime.AddSeconds(timezone).AddHours(1) > sunsetTime)
                                    weatherCd = weatherCd + "_n";
                                break;
                            }
                        }

                    }
                    else
                    {
                        var array = result["daily"].Value<JArray>();
                        List<WeatherInfo> forecasts = array.ToObject<List<WeatherInfo>>();
                        foreach (var item in forecasts)
                        {
                            var weatime = UnixTimeStampToDateTime(item.dt, 0);
                            if (evntTime.AddSeconds(timezone).Date.Equals(weatime.AddSeconds(timezone).Date))
                            {
                                if (evntTime.AddSeconds(timezone).AddHours(1) > UnixTimeStampToDateTime(item.sunset, timezone))
                                {
                                    weatherCd = item.weather[0].id + "_n";
                                    temp = Math.Round(double.Parse(((JObject)item.temp)["night"].ToString()) - 273.15, 0);
                                }
                                else
                                {
                                    weatherCd = item.weather[0].id;
                                    temp = Math.Round(double.Parse(((JObject)item.temp)["day"].ToString()) - 273.15, 0);
                                }
                                break;
                            }
                        }
                    }
                }

                return new JObject()
                {
                    { "title", title },
                    { "session", session },
                    { "date", date },
                    { "time", time},
                    { "circuit", circuit},
                    { "url", circuit_url },
                    { "flag", flag_url },
                    { "weather", "_" + weatherCd },
                    { "temp", weatherCd.Equals("000")? "" : temp + "°C" }
                }.ToString();
            }
            catch (Exception ex)
            {
                return new JObject()
                    {
                        { "title", ex.Message },
                        { "session", "ERROR" },
                        { "date", "---. -, ----" },
                        { "time", "--:--"},
                        { "circuit", "TBA"},
                        { "url", "http://210.2.41.217:8880/files/kbo/emblems?team=TBA" },
                        { "flag", "" },
                        { "weather", "_000" },
                        { "temp", "" }
                    }.ToString();
            }
            
        }


        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp, double shift)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp+shift);
            return dtDateTime;
        }
    }
}
