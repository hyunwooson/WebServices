using System;
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
using HtmlAgilityPack;
using System.Data;
using Ical.Net.CalendarComponents;
using System.Xml;
using Newtonsoft.Json;

namespace WebServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class F1Controller : ControllerBase
    {
        #region Result Json class from weatherAPI
        public class Weather
        {
            public string id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
            public bool night { get; set; }
            public string weatherCode { get { return id + (night ? "_n" : ""); } }
            private string _temp = "";
            public string temp { get { return _temp; } set { _temp = value + "°C"; } }
        }

        public class Current
        {
            public int dt { get; set; }
            public int sunrise { get; set; }
            public int sunset { get; set; }
            public double temp { get; set; }
            public double feels_like { get; set; }
            public int pressure { get; set; }
            public int humidity { get; set; }
            public double dew_point { get; set; }
            public double uvi { get; set; }
            public int clouds { get; set; }
            public int visibility { get; set; }
            public double wind_speed { get; set; }
            public int wind_deg { get; set; }
            public List<Weather> weather { get; set; }
        }

        public class Minutely
        {
            public int dt { get; set; }
            public int precipitation { get; set; }
        }

        public class Hourly
        {
            public int dt { get; set; }
            public double temp { get; set; }
            public double feels_like { get; set; }
            public int pressure { get; set; }
            public int humidity { get; set; }
            public double dew_point { get; set; }
            public double uvi { get; set; }
            public int clouds { get; set; }
            public int visibility { get; set; }
            public double wind_speed { get; set; }
            public int wind_deg { get; set; }
            public double wind_gust { get; set; }
            public List<Weather> weather { get; set; }
            public int pop { get; set; }
        }

        public class Temp
        {
            public double day { get; set; }
            public double min { get; set; }
            public double max { get; set; }
            public double night { get; set; }
            public double eve { get; set; }
            public double morn { get; set; }
        }

        public class FeelsLike
        {
            public double day { get; set; }
            public double night { get; set; }
            public double eve { get; set; }
            public double morn { get; set; }
        }

        public class Daily
        {
            public int dt { get; set; }
            public int sunrise { get; set; }
            public int sunset { get; set; }
            public int moonrise { get; set; }
            public int moonset { get; set; }
            public double moon_phase { get; set; }
            public Temp temp { get; set; }
            public FeelsLike feels_like { get; set; }
            public int pressure { get; set; }
            public int humidity { get; set; }
            public double dew_point { get; set; }
            public double wind_speed { get; set; }
            public int wind_deg { get; set; }
            public double wind_gust { get; set; }
            public List<Weather> weather { get; set; }
            public int clouds { get; set; }
            public int pop { get; set; }
            public double uvi { get; set; }
        }

        public class WeatherAPIResult
        {
            public double lat { get; set; }
            public double lon { get; set; }
            public string timezone { get; set; }
            public double timezone_offset { get; set; }
            public Current current { get; set; }
            public List<Minutely> minutely { get; set; }
            public List<Hourly> hourly { get; set; }
            public List<Daily> daily { get; set; }
        }
        #endregion





        public class WeatherInfo
        {
            public double dt;
            public double sunset;
            public string night = "";
            public object temp;
            public List<Weather> weather;
        }
       
        private readonly ILogger<F1Controller> _logger;

        public F1Controller(ILogger<F1Controller> logger)
        {
            _logger = logger;
        }

        class RaceWeek
        {
            public class Session
            {
                public DateTime StartTimeUTC { get; set; }
                public string SessionTitle;
                public CalendarEvent Calendar;
            }

            public int RoundNo;

            public string GpTitle;
            public List<Session> Sessions { get; set; }

            public DateTime Start { get { return Sessions.OrderBy(s => s.StartTimeUTC).FirstOrDefault().StartTimeUTC; } }
            public DateTime End { get { return Sessions.OrderBy(s => s.StartTimeUTC).LastOrDefault().StartTimeUTC; } }

            public RaceWeek()
            {
                Sessions = new List<Session>();
            }

            public override string ToString()
            {
                return GpTitle + " // " + Start.ToString("MMMdd") + " - " + End.ToString("MMMdd");
            }
        }

        [HttpGet("testt")]
        public string GetTestt()
        {
            return new JObject()
                {
                    { "title", "GPTitle" },
                    { "upcoming", "upcoming" },
                    { "date", "date" },
                    { "time", "time" },
                    { "circuit", "circuit"},
                    { "url", "circuit_url" },
                    { "flag", "flag_url" },
                    { "weather", "_" + "weatherCd" },
                    { "temp", "temp" + "°C" }
                }.ToString();
        }

        [HttpGet]
        public string Get(string shift)
        {
            WeatherAPIResult weatherReport;
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

                var webRequest = WebRequest.Create(@"https://calendar.google.com/calendar/ical/ekqk1nbdusr1baon1ic42oeeik%40group.calendar.google.com/public/basic.ics");


                #region [Get GrandPrix Title from Formula1.com]
                HtmlDocument doc = new HtmlDocument();

                string htmlCode = "";

                using (WebClient client = new WebClient())
                {
                    htmlCode = client.DownloadString($"https://www.formula1.com/en/racing/{DateTime.Now.Year}.html");
                }

                doc.LoadHtml(htmlCode);
                var raceCards = doc.DocumentNode.SelectNodes($"//fieldset[contains(@class, 'race-card-wrapper ')]");
                
                Dictionary<int,string> eventTitleDic = new Dictionary<int, string>();

                foreach (var item in raceCards)
                {
                    HtmlDocument itemDoc = new HtmlDocument();
                    itemDoc.LoadHtml(item.OuterHtml);

                    var roundNumber = itemDoc.DocumentNode.SelectSingleNode($"//legend[contains(@class, 'card-title ')]").InnerText;

                    var eventTitle = itemDoc.DocumentNode.SelectSingleNode($"//div[contains(@class, 'event-title ')]").InnerText;

                    eventTitle = System.Web.HttpUtility.HtmlDecode(eventTitle);

                    eventTitleDic.Add(int.Parse(roundNumber.Split(" ")[1]), eventTitle);
                }

                eventTitleDic = eventTitleDic.OrderBy(e => e.Key).ToDictionary(k => k.Key, v => v.Value);
                #endregion



                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    text = reader.ReadToEnd();
                }
                var calendar = Calendar.Load(text);


                string evntName = "";
                var evntTime = new DateTime();
                bool seasonEnd = true;

                var events = calendar.Events.OrderBy(ev => ev.Start.AsUtc).Where(ev => ev.Start.Year == DateTime.Now.Year).ToList();
                var races = events.Where(e => e.Summary.Contains("Race")).OrderBy(r => r.Start).ToList();
                var seasonYear = races[0].Start.AsUtc.Year;
                var eventTitles = eventTitleDic.Select(e => e.Value).ToList();


                List<RaceWeek> SeasonCalendar = new List<RaceWeek>();

                for (int i = 0; i < races.Count; i++)
                {
                    string evntTitle = eventTitles[i];
                    string evntKey = races[i].Summary.Split(":").LastOrDefault().Trim();

                    var weekEvents = events.Where(e => e.Summary.Contains(evntKey)).OrderBy(e=>e.Start).ToList();

                    RaceWeek week = new RaceWeek();
                    week.RoundNo = i + 1;
                    week.GpTitle = evntTitle;
                    for (int w = 0; w < weekEvents.Count; w++)
                    {
                        RaceWeek.Session sssn = new RaceWeek.Session();

                        sssn.SessionTitle = weekEvents[w].Summary.Split(":").FirstOrDefault().Trim();
                        sssn.StartTimeUTC = weekEvents[w].Start.AsUtc;
                        sssn.Calendar = weekEvents[w];
                        week.Sessions.Add(sssn);
                    }

                    SeasonCalendar.Add(week);
                }

                var circuitsaaaa = SeasonCalendar.Select(w => w.Sessions[0].Calendar.Location).ToList();

                RaceWeek upcomingWeek = new RaceWeek();
                RaceWeek.Session upcomingSession = new RaceWeek.Session();

                for (int i = 0; i < SeasonCalendar.Count; i++)
                {
                    if(DateTime.UtcNow < SeasonCalendar[i].End)
                    {
                        upcomingWeek = SeasonCalendar[i];
                        for (int j = 0; j < upcomingWeek.Sessions.Count; j++)
                        {
                            if (DateTime.UtcNow < upcomingWeek.Sessions[j].StartTimeUTC)
                            {
                                upcomingSession = upcomingWeek.Sessions[j];
                                seasonEnd = false;
                                break;
                            }
                        }
                        break;
                    }
                }



                //for (int i = 0; i < events.Count; i++)
                //{
                //    if (DateTime.UtcNow < events[i].Start.AsUtc)
                //    {
                //        evntName = events[i].Summary.Trim();
                //        evntTime = events[i].Start.AsUtc;
                //        circuit = events[i].Location.Trim();
                //        seasonEnd = false;

                //        if (evntName.ToUpper().Contains("CANCEL"))
                //            continue;

                //        break;
                //    }
                //}
                circuit = upcomingSession.Calendar.Location;

                if (seasonEnd)
                {
                    return new JObject()
                    {
                        { "title", $"{seasonYear} FORMULA 1 WORLD CHAMPIONSHIP™" },
                        { "upcoming", "ENDED" },
                        { "session01", "-----" },
                        { "session02", "-----" },
                        { "session03", "-----" },
                        { "session04", "-----" },
                        { "session05", "-----" },
                        { "date", "---. -, ----" },
                        { "time", "--:--"},
                        { "circuit", "TBA"},
                        { "url", "http://210.2.41.217:8880/files/kbo/emblems?team=TBA" },
                        { "flag", "" },
                        { "weather", "_000" },
                        { "temp", "" }
                    }.ToString();
                }

                string title = upcomingWeek.GpTitle;
                //string title = evntName.Substring(0, evntName.IndexOf(DateTime.Now.Year.ToString()) - 1);

                string session = evntName.Split(new char[] { '–', '-' }).Last().Trim();
                evntTime = upcomingSession.StartTimeUTC;
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

                weatherReport = result.ToObject<WeatherAPIResult>();
                weatherReport.minutely = weatherReport.minutely.OrderBy(m => m.dt).ToList();
                weatherReport.daily = weatherReport.daily.OrderBy(d => d.dt).ToList();
                weatherReport.hourly = weatherReport.hourly.OrderBy(h => h.dt).ToList();

                string weatherCd = "000";
                double temp = 0;

                #region cccc
                //if (!result.TryGetValue("cod", out JToken jt))
                //{
                //    double timezone = result["timezone_offset"].Value<double>();

                //    if (date.Equals("Today") || date.Equals("Tomorrow"))
                //    {
                //        var array = result["hourly"].Value<JArray>();
                //        List<WeatherInfo> forecasts = array.ToObject<List<WeatherInfo>>();
                //        foreach (var item in forecasts)
                //        {
                //            var weatime = UnixTimeStampToDateTime(item.dt, 0);
                //            if (evntTime < weatime)
                //            {
                //                weatherCd = item.weather[0].id;
                //                temp = Math.Round(double.Parse(item.temp.ToString()) - 273.15, 0);
                //                break;
                //            }
                //        }
                //        var array_d = result["daily"].Value<JArray>();
                //        List<WeatherInfo> forecasts_d = array_d.ToObject<List<WeatherInfo>>();
                //        foreach (var item in forecasts_d)
                //        {
                //            var weatime = UnixTimeStampToDateTime(item.dt, 0);
                //            if (evntTime.AddSeconds(timezone).Date.Equals(weatime.AddSeconds(timezone).Date))
                //            {
                //                var sunsetTime = UnixTimeStampToDateTime(item.sunset, timezone);
                //                if (evntTime.AddSeconds(timezone).AddHours(1) > sunsetTime)
                //                    weatherCd = weatherCd + "_n";
                //                break;
                //            }
                //        }

                //    }
                //    else
                //    {
                //        var array = result["daily"].Value<JArray>();
                //        List<WeatherInfo> forecasts = array.ToObject<List<WeatherInfo>>();
                //        foreach (var item in forecasts)
                //        {
                //            var weatime = UnixTimeStampToDateTime(item.dt, 0);
                //            if (evntTime.AddSeconds(timezone).Date.Equals(weatime.AddSeconds(timezone).Date))
                //            {
                //                if (evntTime.AddSeconds(timezone).AddHours(1) > UnixTimeStampToDateTime(item.sunset, timezone))
                //                {
                //                    weatherCd = item.weather[0].id + "_n";
                //                    temp = Math.Round(double.Parse(((JObject)item.temp)["night"].ToString()) - 273.15, 0);
                //                }
                //                else
                //                {
                //                    weatherCd = item.weather[0].id;
                //                    temp = Math.Round(double.Parse(((JObject)item.temp)["day"].ToString()) - 273.15, 0);
                //                }
                //                break;
                //            }
                //        }
                //    }
                //}
                #endregion

                var session01_weather = GetWeatherForecast(weatherReport, upcomingWeek.Sessions[0].StartTimeUTC);
                var session02_weather = GetWeatherForecast(weatherReport, upcomingWeek.Sessions[1].StartTimeUTC);
                var session03_weather = GetWeatherForecast(weatherReport, upcomingWeek.Sessions[2].StartTimeUTC);
                var session04_weather = GetWeatherForecast(weatherReport, upcomingWeek.Sessions[3].StartTimeUTC);
                var session05_weather = GetWeatherForecast(weatherReport, upcomingWeek.Sessions[4].StartTimeUTC);


                Dictionary<int, bool> sessionFinish = new Dictionary<int, bool>() 
                {
                    { 0, false},
                    { 1, false},
                    { 2, false},
                    { 3, false},
                    { 4, false},
                };


                string[] GridPosition = new string[20];
                for (int i = 0; i < sessionFinish.Count; i++)
                {
                    if (upcomingWeek.Sessions[i].SessionTitle.StartsWith("FP"))
                    {
                        sessionFinish[i] = DateTime.UtcNow > upcomingWeek.Sessions[i].StartTimeUTC.AddHours(1);
                    }
                    else if (upcomingWeek.Sessions[i].SessionTitle.StartsWith("Qual"))
                    {
                        string f1ApiUrl = $"http://ergast.com/api/f1/{seasonYear}/{upcomingWeek.RoundNo}/qualifying";
                        HttpWebRequest qualifyingRequest = (HttpWebRequest)WebRequest.Create(f1ApiUrl);
                        qualifyingRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                        string qualiResult = "";

                        using (HttpWebResponse response = (HttpWebResponse)qualifyingRequest.GetResponse())
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            qualiResult = reader.ReadToEnd();
                            XmlDocument docu = new XmlDocument();
                            docu.LoadXml(qualiResult);
                            if (qualiResult.Contains("QualifyingResult"))
                            {
                                sessionFinish[i] = true;
                            }

                            var lines = qualiResult.Split("\n").Where(l => l.Contains("<Driver driverId=")).ToList();
                            // curating results
                            //
                            //
                            //
                            for (int g = 0; g < lines.Count; g++)
                            {
                                GridPosition[g] = lines[g].Substring(lines[g].IndexOf("code=\"") + 6, 3);
                            }
                        }


                    }
                    else if (upcomingWeek.Sessions[i].SessionTitle.StartsWith("Sprint"))
                    {

                    }
                }


                var upcoming_weather = GetWeatherForecast(weatherReport, upcomingSession.StartTimeUTC);

                return new JObject()
                {
                    { "title", upcomingWeek.GpTitle },
                    { "upcoming", upcomingSession.SessionTitle },
                    { "date", date },
                    { "time", time },
                    { "s01", upcomingWeek.Sessions[0].SessionTitle },
                    { "s01_date", GetDateString(upcomingWeek.Sessions[0].StartTimeUTC, timeShift) },
                    { "s01_time", upcomingWeek.Sessions[0].StartTimeUTC.AddSeconds(timeShift).ToString("HH:mm") },
                    { "s02", upcomingWeek.Sessions[1].SessionTitle },
                    { "s02_date", GetDateString(upcomingWeek.Sessions[1].StartTimeUTC, timeShift) },
                    { "s02_time", upcomingWeek.Sessions[1].StartTimeUTC.AddSeconds(timeShift).ToString("HH:mm") },
                    { "s03", upcomingWeek.Sessions[2].SessionTitle },
                    { "s03_date", GetDateString(upcomingWeek.Sessions[2].StartTimeUTC, timeShift) },
                    { "s03_time", upcomingWeek.Sessions[2].StartTimeUTC.AddSeconds(timeShift).ToString("HH:mm") },
                    { "s04", upcomingWeek.Sessions[3].SessionTitle },
                    { "s04_date", GetDateString(upcomingWeek.Sessions[3].StartTimeUTC, timeShift) },
                    { "s04_time", upcomingWeek.Sessions[3].StartTimeUTC.AddSeconds(timeShift).ToString("HH:mm") },
                    { "s05", upcomingWeek.Sessions[4].SessionTitle },
                    { "s05_date", GetDateString(upcomingWeek.Sessions[4].StartTimeUTC, timeShift) },
                    { "s05_time", upcomingWeek.Sessions[4].StartTimeUTC.AddSeconds(timeShift).ToString("HH:mm") },
                    { "circuit", circuit},
                    { "url", circuit_url },
                    { "flag", flag_url },
                    { "weather", (upcoming_weather == null)? "_000" : "_" + upcoming_weather.weatherCode },
                    { "temp", (upcoming_weather == null)? "" : upcoming_weather.temp },
                    //{ "temp", weatherCd.Equals("000")? "" : temp + "°C" },
                    { "s01_weather", (session01_weather==null)? "_000" : "_" + session01_weather.weatherCode},
                    { "s02_weather", (session02_weather==null)? "_000" : "_" + session02_weather.weatherCode},
                    { "s03_weather", (session03_weather==null)? "_000" : "_" + session03_weather.weatherCode},
                    { "s04_weather", (session04_weather==null)? "_000" : "_" + session04_weather.weatherCode},
                    { "s05_weather", (session05_weather==null)? "_000" : "_" + session05_weather.weatherCode},
                    { "s01_temp", (session01_weather==null)? "" : session01_weather.temp},
                    { "s02_temp", (session02_weather==null)? "" : session02_weather.temp},
                    { "s03_temp", (session03_weather==null)? "" : session03_weather.temp},
                    { "s04_temp", (session04_weather==null)? "" : session04_weather.temp},
                    { "s05_temp", (session05_weather==null)? "" : session05_weather.temp},
                    { "s01_stat", sessionFinish[0] },
                    { "s02_stat", sessionFinish[1] },
                    { "s03_stat", sessionFinish[2] },
                    { "s04_stat", sessionFinish[3] },
                    { "s05_stat", sessionFinish[4] },
                    { "grid_0" , GridPosition[0] },
                    { "grid_1" , GridPosition[1] },
                    { "grid_2" , GridPosition[2] },
                    { "grid_3" , GridPosition[3] },
                    { "grid_4" , GridPosition[4] },
                    { "grid_5" , GridPosition[5] },
                    { "grid_6" , GridPosition[6] },
                    { "grid_7" , GridPosition[7] },
                    { "grid_8" , GridPosition[8] },
                    { "grid_9" , GridPosition[9] },
                    { "grid_10" , GridPosition[10] },
                    { "grid_11" , GridPosition[11] },
                    { "grid_12" , GridPosition[12] },
                    { "grid_13" , GridPosition[13] },
                    { "grid_14" , GridPosition[14] },
                    { "grid_15" , GridPosition[15] },
                    { "grid_16" , GridPosition[16] },
                    { "grid_17" , GridPosition[17] },
                    { "grid_18" , GridPosition[18] },
                    { "grid_19" , GridPosition[19] },
                    { "updDt" , DateTime.UtcNow.AddSeconds(timeShift).ToString("M/d/yyyy HH:mm:ss")}
                }.ToString();
            }
            catch (Exception ex)
            {
                return new JObject()
                    {
                        { "title", ex.Message },
                        { "upcoming", "ERROR" },
                        { "date", "---. -, ----" },
                        { "time", "--:--" },
                        { "s01", "ERROR" },
                        { "s01_date", "---. --" },
                        { "s01_time", "--:--" },
                        { "s02", "ERROR" },
                        { "s02_date", "---. --" },
                        { "s02_time", "--:--" },
                        { "s03", "ERROR" },
                        { "s03_date", "---. --" },
                        { "s03_time", "--:--" },
                        { "s04", "ERROR" },
                        { "s04_date", "---. --" },
                        { "s04_time", "--:--" },
                        { "s05", "ERROR" },
                        { "s05_date", "---. --" },
                        { "s05_time", "--:--" },
                        { "circuit", "ERROR"},
                        { "url", "http://sonamoo456.iptime.org:8880/files/kbo/emblems?team=TBA" },
                        { "flag", "" },
                        { "weather", "_000" },
                        { "temp", "" },
                        { "s01_weather", "_000" },
                        { "s02_weather", "_000" },
                        { "s03_weather", "_000" },
                        { "s04_weather", "_000" },
                        { "s05_weather", "_000" },
                        { "s01_temp", "" },
                        { "s02_temp", "" },
                        { "s03_temp", "" },
                        { "s04_temp", "" },
                        { "s05_temp", "" },
                        }.ToString();
            }

        }

        private string GetDateString(DateTime datetimeUTC, int timeShift)
        {
            if (datetimeUTC.AddSeconds(timeShift).Date == DateTime.UtcNow.AddSeconds(timeShift).Date)
                return "Today";
            else if (datetimeUTC.AddSeconds(timeShift).Date == DateTime.UtcNow.AddDays(1).AddSeconds(timeShift).Date)
                return "Tomorrow";
            else
                return datetimeUTC.AddSeconds(timeShift).ToString("MMM. d");
        }
        private Weather GetWeatherForecast(WeatherAPIResult weatherReport, DateTime dtUtc)
        {
            var timezone = weatherReport.timezone_offset;
            //hourly
            for (int i = 0; i < weatherReport.hourly.Count; i++)
            {
                var forcastTime = UnixTimeStampToDateTime(weatherReport.hourly[i].dt, 0);
                if (dtUtc < forcastTime)
                {
                    weatherReport.hourly[i].weather[0].temp = Math.Round(weatherReport.hourly[i].temp - 273.15, 0).ToString();

                    var sunsetTime = UnixTimeStampToDateTime(weatherReport.current.sunset, timezone);
                    if (forcastTime.AddSeconds(timezone).AddHours(1) > sunsetTime)
                        weatherReport.hourly[i].weather[0].night = true;
                    return weatherReport.hourly[i].weather[0];
                }
            }
            for (int i = 0; i < weatherReport.daily.Count; i++)
            {

                var weatime = UnixTimeStampToDateTime(weatherReport.daily[i].dt, 0);
                if (dtUtc.AddSeconds(timezone).Date.Equals(weatime.AddSeconds(timezone).Date))
                {

                    var sunsetTime = UnixTimeStampToDateTime(weatherReport.daily[i].sunset, timezone);
                    if (dtUtc.AddSeconds(timezone).AddHours(1) > sunsetTime)
                    {
                        weatherReport.daily[i].weather[0].night = true;
                        weatherReport.daily[i].weather[0].temp = Math.Round(weatherReport.daily[i].temp.night - 273.15, 0).ToString();
                    }
                    else
                    {
                        weatherReport.daily[i].weather[0].temp = Math.Round(weatherReport.daily[i].temp.day - 273.15, 0).ToString();
                    }
                    return weatherReport.daily[i].weather[0];
                }
            }
            return null;
        }

        private class DriverInfo
        {
            public int Pos { get; set; }
            public string Name { get; set; }
            public string Nationality { get; set; }
            public string Car { get; set; }
            public float PTS { get; set; }

            public string ConcatData()
            {
                string _s = "";
                _s += Pos + ";";
                _s += Name + ";";
                _s += Nationality + ";";
                _s += Car + ";";
                _s += GetTeamColorHex(Car) + ";";
                _s += PTS + ";";
                _s += GetFlag(Nationality) + ";";
                return _s;
            }

            private string GetTeamColorHex(string team)
            {
                switch (team)
                {
                    case "Mercedes":
                        return "#00D2BE";
                    case "Red Bull":
                    case "Red Bull Racing RBPT":
                        return "#0600EF";
                    case "McLaren":
                    case "McLaren Mercedes":
                        return "#FF8700";
                    case "Ferrari":
                        return "#DC0000";
                    case "Alpine F1 Team":
                    case "Alpine Renault":
                        return "#0090FF";
                    case "AlphaTauri":
                    case "AlphaTauri RBPT":
                        return "#2B4562";
                    case "Aston Martin":
                    case "Aston Martin Aramco Mercedes":
                        return "#006F62";
                    case "Haas F1 Team":
                    case "Haas Ferrari":
                        return "#B6BABD";
                    case "Williams":
                    case "Williams Mercedes":
                        return "#005AFF";
                    case "Alfa Romeo":
                    case "Alfa Romeo Ferrari":
                        return "#900000";
                    default:
                        return "#000000";
                }
            }
            
            private string GetTeamLogo(string team)
            {
                switch (team)
                {
                    case "Mercedes":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/mercedes-logo.png.transform/2col/image.png";
                    case "Red Bull":
                    case "Red Bull Racing RBPT":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/red-bull-racing-logo.png.transform/2col/image.png";
                    case "McLaren":
                    case "McLaren Mercedes":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/mclaren-logo.png.transform/2col/image.png";
                    case "Ferrari":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/ferrari-logo.png.transform/2col/image.png";
                    case "Alpine F1 Team":
                    case "Alpine Renault":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/alpine-logo.png.transform/2col/image.png";
                    case "AlphaTauri":
                    case "AlphaTauri RBPT":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/alphatauri-logo.png.transform/2col/image.png";
                    case "Aston Martin":
                    case "Aston Martin Aramco Mercedes":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/aston-martin-logo.png.transform/2col/image.png";
                    case "Haas F1 Team":
                    case "Haas Ferrari":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/haas-f1-team-logo.png.transform/2col/image.png";
                    case "Williams":
                    case "Williams Mercedes":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/williams-logo.png.transform/2col/image.png";
                    case "Alfa Romeo":
                    case "Alfa Romeo Ferrari":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/alfa-romeo-racing-logo.png.transform/2col/image.png";
                    default:
                        return "000000";
                }
            }
            private string GetFlag(string nationality)
            {
                Dictionary<string, string> _flagDict = new Dictionary<string, string>()
                {
                    { "AUS", "https://cdn.countryflags.com/thumbs/australia/flag-square-250.png" },
                    { "CAN", "https://cdn.countryflags.com/thumbs/canada/flag-square-250.png" },
                    { "ESP", "https://cdn.countryflags.com/thumbs/spain/flag-square-250.png" },
                    { "FIN", "https://cdn.countryflags.com/thumbs/finland/flag-square-250.png" },
                    { "FRA", "https://cdn.countryflags.com/thumbs/france/flag-square-250.png" },
                    { "GBR", "https://cdn.countryflags.com/thumbs/united-kingdom/flag-square-250.png" },
                    { "GER", "https://cdn.countryflags.com/thumbs/germany/flag-square-250.png" },
                    { "ITA", "https://cdn.countryflags.com/thumbs/italia/flag-square-250.png" },
                    { "JPN", "https://cdn.countryflags.com/thumbs/japan/flag-square-250.png" },
                    { "MEX", "https://cdn.countryflags.com/thumbs/mexico/flag-square-250.png" },
                    { "MON", "https://cdn.countryflags.com/thumbs/monaco/flag-square-250.png" },
                    { "NED", "https://cdn.countryflags.com/thumbs/netherlands/flag-square-250.png" },
                    { "RAF", "https://cdn.countryflags.com/thumbs/russia/flag-square-250.png" },
                    { "THA", "https://cdn.countryflags.com/thumbs/thailand/flag-square-250.png" },
                    { "DEN", "https://cdn.countryflags.com/thumbs/denmark/flag-square-250.png" },
                    { "CHN", "https://cdn.countryflags.com/thumbs/china/flag-square-250.png" },
                };

                return _flagDict[nationality];
            }
        }

        [HttpGet("constructor")]
        public string GetConstructorStandings()
        {
            var result = new JObject();
            try
            {
                HtmlDocument doc = new HtmlDocument();
                HtmlNode targetNode = null;

                string htmlCode = "";

                HtmlNode tableNode;
                List<DriverInfo> DriverStandings = new List<DriverInfo>();
                int thisYear = DateTime.Now.Year;
                do
                {
                    using (WebClient client = new WebClient())
                    {
                        htmlCode = client.DownloadString($"https://www.formula1.com/en/results.html/{thisYear}/team.html");
                    }

                    doc.LoadHtml(htmlCode);

                    var headers = doc.DocumentNode.SelectNodes("//tr/th");


                    tableNode = doc.DocumentNode.SelectSingleNode("//table[@class='resultsarchive-table']");

                } while (tableNode == null);

                foreach (var row in tableNode.SelectNodes("//tr[td]"))
                {
                    var _arr = row.SelectNodes("td").Select(td => td.InnerText).ToArray();
                    var point = _arr[3];
                    var str = ConvertTeamName(_arr[2].Trim()) + ";" + GetCarImageUrl(_arr[2].Trim()) + ";" + GetTeamColor(_arr[2].Trim()) + ";" + point + ";" + GetConstructorLogo(_arr[2].Trim());
                    result.Add($"pos{_arr[1].Trim()}", str);
                }


                //string f1ApiUrl = $"http://ergast.com/api/f1/current/constructorStandings";
                //HttpWebRequest standingRequest = (HttpWebRequest)WebRequest.Create(f1ApiUrl);
                //standingRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                //string standingResult = "";

                //using (HttpWebResponse response = (HttpWebResponse)standingRequest.GetResponse())
                //using (Stream stream = response.GetResponseStream())
                //using (StreamReader reader = new StreamReader(stream))
                //{
                //    standingResult = reader.ReadToEnd();
                //    XmlDocument docu = new XmlDocument();
                //    docu.LoadXml(standingResult);
                    
                //    var points = standingResult.Split("\n").Where(l => l.Contains("<ConstructorStanding position=")).Select(l=>l.Trim()).ToList();
                //    var lines = standingResult.Split("\n").Where(l => l.Contains("<Name>")).Select(l => l.Replace("<Name>", "").Replace("</Name>", "").Trim()).ToList();

                //    for (int i = 0; i < lines.Count; i++)
                //    {
                //        var point = points[i].Split("\"")[5];
                //        var str = lines[i] + ";" + GetCarImageUrl(lines[i]) + ";" + GetTeamColor(lines[i]) + ";" + point;
                //        result.Add($"pos{i+1}", str);
                //    }
                //}
            }
            catch (Exception)
            {

                throw;
            }
            return result.ToString();
        }
        private string GetConstructorLogo(string team)
        {
            switch (team)
            {
                case "Mercedes":
                    return "https://www.carlogos.org/car-logos/mercedes-benz-logo.png";
                case "Red Bull":
                case "Red Bull Racing RBPT":
                    return "https://w7.pngwing.com/pngs/424/256/png-transparent-redbull-logo-red-bull-energy-drink-desktop-krating-daeng-logo-red-bull-mammal-carnivoran-orange.png";
                case "McLaren":
                case "McLaren Mercedes":
                    return "https://www.formula1.com/content/dam/fom-website/teams/2021/mclaren-logo.png.transform/2col/image.png";
                case "Ferrari":
                    return "https://www.carlogos.org/car-logos/scuderia-ferrari-logo-800x1050.png";
                case "Alpine F1 Team":
                case "Alpine Renault":
                    return "https://www.formula1.com/content/dam/fom-website/teams/2021/alpine-logo.png.transform/2col/image.png";
                case "AlphaTauri":
                case "AlphaTauri RBPT":
                    return "https://cdn.freelogovectors.net/wp-content/uploads/2021/08/alphatauri_logo-freelogovectors.net_.png";
                case "Aston Martin":
                case "Aston Martin Aramco Mercedes":
                    return "https://www.formula1.com/content/dam/fom-website/teams/2021/aston-martin-logo.png.transform/2col/image.png";
                case "Haas F1 Team":
                case "Haas Ferrari":
                    return "https://upload.wikimedia.org/wikipedia/commons/d/d4/Logo_Haas_F1.png";
                case "Williams":
                case "Williams Mercedes":
                    return "https://www.formula1.com/content/dam/fom-website/teams/2021/williams-logo.png.transform/2col/image.png";
                case "Alfa Romeo":
                case "Alfa Romeo Ferrari":
                    return "https://www.formula1.com/content/dam/fom-website/teams/2021/alfa-romeo-racing-logo.png.transform/2col/image.png";
                default:
                    return "000000";
            }
        }
        private string GetTeamColor(string team)
        {
            switch (team)
            {
                case "Mercedes":
                    return "#00D2BE";
                case "Red Bull":
                case "Red Bull Racing RBPT":
                    return "#0600EF";
                case "McLaren":
                case "McLaren Mercedes":
                    return "#FF8700";
                case "Ferrari":
                    return "#DC0000";
                case "Alpine F1 Team":
                case "Alpine Renault":
                    return "#0090FF";
                case "AlphaTauri":
                case "AlphaTauri RBPT":
                    return "#2B4562";
                case "Aston Martin":
                case "Aston Martin Aramco Mercedes":
                    return "#006F62";
                case "Haas F1 Team":
                case "Haas Ferrari":
                    return "#B6BABD";
                case "Williams":
                case "Williams Mercedes":
                    return "#005AFF";
                case "Alfa Romeo":
                case "Alfa Romeo Ferrari":
                    return "#900000";
                default:
                    return "#000000";
            }
        }
        private string GetCarImageUrl(string team)
        {
            string url = "";
            switch (team)
            {
                case "Ferrari":
                    url = "https://www.formula1.com/content/dam/fom-website/teams/2022/ferrari.png.transform/4col/image.png";
                    break;
                case "Mercedes":
                    url = "https://www.formula1.com/content/dam/fom-website/teams/2022/mercedes.png.transform/4col/image.png";
                    break;
                case "Haas Ferrari":
                case "Haas F1 Team":
                    url = "https://www.formula1.com/content/dam/fom-website/teams/2022/haas-f1-team.png.transform/4col/image.png";
                    break;
                case "Alfa Romeo Ferrari":
                case "Alfa Romeo":
                    url = "https://www.formula1.com/content/dam/fom-website/teams/2022/alfa-romeo.png.transform/4col/image.png";
                    break;
                case "Alpine Renault":
                case "Alpine F1 Team":
                    url = "https://www.formula1.com/content/dam/fom-website/teams/2022/alpine.png.transform/4col/image.png";
                    break;
                case "AlphaTauri RBPT":
                case "AlphaTauri":
                    url = "https://www.formula1.com/content/dam/fom-website/teams/2022/alphatauri.png.transform/4col/image.png";
                    break;
                case "Aston Martin Aramco Mercedes":
                case "Aston Martin":
                    url = "https://www.formula1.com/content/dam/fom-website/teams/2022/aston-martin.png.transform/4col/image.png";
                    break;
                case "Williams Mercedes":
                case "Williams":
                    url = "https://www.formula1.com/content/dam/fom-website/teams/2022/williams.png.transform/4col/image.png";
                    break;
                case "McLaren Mercedes":
                case "McLaren":
                    url = "https://www.formula1.com/content/dam/fom-website/teams/2022/mclaren.png.transform/4col/image.png";
                    break;
                case "Red Bull Racing RBPT":
                case "Red Bull":
                    url = "https://www.formula1.com/content/dam/fom-website/teams/2022/red-bull-racing.png.transform/4col/image.png";
                    break;
                default:
                    break;
            }

  
            return url;
        }
        private string ConvertTeamName(string team)
        {
            switch (team)
            {
                case "Mercedes":
                    return "Mercedes";
                case "Red Bull":
                case "Red Bull Racing RBPT":
                    return "Red Bull";
                case "McLaren":
                case "McLaren Mercedes":
                    return "McLaren";
                case "Ferrari":
                    return "Ferrari";
                case "Alpine F1 Team":
                case "Alpine Renault":
                    return "Alpine Renault";
                case "AlphaTauri":
                case "AlphaTauri RBPT":
                    return "AlphaTauri";
                case "Aston Martin":
                case "Aston Martin Aramco Mercedes":
                    return "Aston Martin";
                case "Haas F1 Team":
                case "Haas Ferrari":
                    return "Haas F1 Team";
                case "Williams":
                case "Williams Mercedes":
                    return "Williams";
                case "Alfa Romeo":
                case "Alfa Romeo Ferrari":
                    return "Alfa Romeo";
                default:
                    return "000000";
            }
        }


        [HttpGet("driver")]
        public string GetDriverStandings()
        {
            var result = new JObject();

            try
            {
                HtmlDocument doc = new HtmlDocument();
                HtmlNode targetNode = null;

                string htmlCode = "";

                HtmlNode tableNode;
                List<DriverInfo> DriverStandings = new List<DriverInfo>();
                int thisYear = DateTime.Now.Year;
                do
                {
                    using (WebClient client = new WebClient())
                    {
                        htmlCode = client.DownloadString($"https://www.formula1.com/en/results.html/{thisYear}/drivers.html");
                    }

                    doc.LoadHtml(htmlCode);

                    var headers = doc.DocumentNode.SelectNodes("//tr/th");


                    tableNode = doc.DocumentNode.SelectSingleNode("//table[@class='resultsarchive-table']");

                } while (tableNode == null);

                foreach (var row in tableNode.SelectNodes("//tr[td]"))
                {
                    if (DriverStandings.Count >= 20)
                        break;

                    var _arr = row.SelectNodes("td").Select(td => td.InnerText).ToArray();

                    var _drv = new DriverInfo();
                    DriverStandings.Add(_drv);

                    _drv.Pos = int.Parse(_arr[1].ToString().Trim());

                    string name = _arr[2].ToString().Trim();
                    _drv.Name = name.Substring(name.Length - 3, 3);

                    _drv.Nationality = _arr[3].ToString().Trim();

                    _drv.Car = _arr[4].ToString().Trim();

                    _drv.PTS = float.Parse(_arr[5].ToString().Trim());
                }


                for (int i = 0; i < DriverStandings.Count; i++)
                {
                    result.Add($"pos{i+1}", DriverStandings[i].ConcatData());
                }
            }
            catch (Exception)
            {
                result = new JObject()
                {
                    { "pos01" , "1;ERR;;;;;"},
                    { "pos02" , "2;ERR;;;;;"},
                    { "pos03" , "3;ERR;;;;;"},
                    { "pos04" , "4;ERR;;;;;"},
                    { "pos05" , "5;ERR;;;;;"},
                    { "pos06" , "6;ERR;;;;;"},
                    { "pos07" , "7;ERR;;;;;"},
                    { "pos08" , "8;ERR;;;;;"},
                    { "pos09" , "9;ERR;;;;;"},
                    { "pos10" , "10;ERR;;;;;"},
                };
            }
            return result.ToString();
        }


        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp, double shift)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp+shift);
            return dtDateTime;
        }

    }
}
