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
            if (!Int32.TryParse(shift, out int timeShift))
            {
                timeShift = 0;
            }

            string circuit = "";
            string lateral = "";
            string longitudinal = "";

            string text = "";

            var webRequest = WebRequest.Create(@"http://www.formula1.com/calendar/Formula_1_Official_Calendar.ics");

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                text = reader.ReadToEnd();
            }
            var calendar = Calendar.Load(text);

            string evntName = "";
            var evntTime = new DateTime();
            for (int i = 0; i < calendar.Events.Count; i++)
            {
                if (DateTime.UtcNow < calendar.Events[i].Start.AsUtc)
                {
                    evntName = calendar.Events[i].Summary;
                    evntTime = calendar.Events[i].Start.AsUtc;
                    circuit = calendar.Events[i].Location;
                    var loc = Properties.F1.Resource.ResourceManager.GetObject(circuit.ToUpper().Replace(' ', '_')).ToString().Split(',');
                    lateral = loc[0];
                    longitudinal = loc[1];
                    break;
                }
            }

            string title = evntName.Substring(0, evntName.IndexOf("2020") - 1);

            string session = evntName.Substring(evntName.IndexOf("-") + 2);

            string date;
            if (evntTime.AddSeconds(timeShift).Date == DateTime.UtcNow.AddSeconds(timeShift).Date)
                date = "Today";
            else if (evntTime.AddSeconds(timeShift).Date == DateTime.UtcNow.AddDays(1).AddSeconds(timeShift).Date)
                date = "Tomorrow";
            else
                date = evntTime.AddSeconds(timeShift).ToString("MMM. d, yyyy");

            string time = evntTime.AddSeconds(timeShift).ToString("HH:mm");



            string url;

            string flag;
            switch (evntName)
            {
                case string a when a.Contains("PORTUGAL"):
                    url = "https://www.formula1.com/content/dam/fom-website/2018-redesign-assets/Track%20icons%204x3/Portugal%20carbon.png.transform/3col/image.png";
                    flag = "https://cdn.countryflags.com/thumbs/portugal/flag-wave-250.png";
                    break;
                case string a when a.Contains("ROMAGNA"):
                    url = "https://www.formula1.com/content/dam/fom-website/2018-redesign-assets/Track%20icons%204x3/Emilia%20Romagna%20carbon.png.transform/3col/image.png";
                    flag = "https://cdn.countryflags.com/thumbs/italy/flag-wave-250.png";
                    break;
                case string a when a.Contains("TURKISH"):
                    url = "https://www.formula1.com/content/dam/fom-website/2018-redesign-assets/Track%20icons%204x3/Turkey%20carbon.png.transform/3col/image.png";
                    flag = "https://cdn.countryflags.com/thumbs/turkey/flag-wave-250.png";
                    break;
                case string a when a.Contains("BAHRAIN"):
                    url = "https://www.formula1.com/content/dam/fom-website/2018-redesign-assets/Track%20icons%204x3/Bahrain%20carbon.png.transform/3col/image.png";
                    flag = "https://cdn.countryflags.com/thumbs/bahrain/flag-wave-250.png";
                    break;
                case string a when a.Contains("SAKHIR"):
                    url = "https://www.formula1.com/content/dam/fom-website/2018-redesign-assets/Track%20icons%204x3/Sakhir%20carbon.png.transform/3col/image.png";
                    flag = "https://cdn.countryflags.com/thumbs/bahrain/flag-wave-250.png";
                    break;
                case string a when a.Contains("DHABI"):
                    url = "https://www.formula1.com/content/dam/fom-website/2018-redesign-assets/Track%20icons%204x3/Abu%20Dhabi%20carbon.png.transform/2col/image.png";
                    flag = "https://cdn.countryflags.com/thumbs/united-arab-emirates/flag-wave-250.png";
                    break;
                case string a when a.Contains("EIFEL"):
                    url = "https://www.formula1.com/content/dam/fom-website/2018-redesign-assets/Track%20icons%204x3/Germany%20carbon.png.transform/3col/image.png";
                    flag = "https://cdn.countryflags.com/thumbs/germany/flag-wave-250.png";
                    break;
                default:
                    url = "";
                    flag = "";
                    break;
            }

            string owmUrl = $"https://api.openweathermap.org/data/2.5/onecall?lat={lateral}&lon={longitudinal}&appid=14946c8bd54652131af03989ad323e19&unit=metric";
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

            string weatherCd = "";



            if (result.TryGetValue("cod",out JToken jt))
            {
            }
            else
            {
                double timezone = result["timezone_offset"].Value<double>();

                if (date.Equals("Today")|| date.Equals("Tomorrow"))
                {
                    var array = result["hourly"].Value<JArray>();
                    List<WeatherInfo> forecasts = array.ToObject<List<WeatherInfo>>();
                    foreach (var item in forecasts)
                    {
                        var weatime = UnixTimeStampToDateTime(item.dt, 0);
                        if (evntTime < weatime)
                        {
                            weatherCd = item.weather[0].id;
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
                                weatherCd = item.weather[0].id + "_n";
                            else
                                weatherCd = item.weather[0].id;
                            break;
                        }
                    }
                    weatherCd = "_000";
                }
            }



            return new JObject()
            {
                { "title", title },
                { "session", session },
                { "date", date },
                { "time", time},
                { "circuit", circuit},
                { "url", url },
                { "flag", flag },
                { "weather", weatherCd }
            }.ToString();
        }
        
        [HttpGet("test")]
        public string GetTest([FromQuery]string shift)
        {
            return "Passed parameter is \'" + shift + "\'";
        }


        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp, double shift)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp+shift);
            return dtDateTime;
        }
    }
}
