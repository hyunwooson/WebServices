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
                    case "Red Bull Racing Honda":
                        return "#0600EF";
                    case "McLaren Mercedes":
                        return "#FF8700";
                    case "Ferrari":
                        return "#DC0000";
                    case "Alpine Renault":
                        return "#0090FF";
                    case "AlphaTauri Honda":
                        return "#2B4562";
                    case "Aston Martin Mercedes":
                        return "#006F62";
                    case "Haas Ferrari":
                        return "#FFFFFF";
                    case "Williams Mercedes":
                        return "#005AFF";
                    case "Alfa Romeo Racing Ferrari":
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
                    case "Red Bull Racing Honda":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/red-bull-racing-logo.png.transform/2col/image.png";
                    case "McLaren Mercedes":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/mclaren-logo.png.transform/2col/image.png";
                    case "Ferrari":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/ferrari-logo.png.transform/2col/image.png";
                    case "Alpine Renault":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/alpine-logo.png.transform/2col/image.png";
                    case "AlphaTauri Honda":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/alphatauri-logo.png.transform/2col/image.png";
                    case "Aston Martin Mercedes":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/aston-martin-logo.png.transform/2col/image.png";
                    case "Haas Ferrari":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/haas-f1-team-logo.png.transform/2col/image.png";
                    case "Williams Mercedes":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/williams-logo.png.transform/2col/image.png";
                    case "Alfa Romeo Racing Ferrari":
                        return "https://www.formula1.com/content/dam/fom-website/teams/2021/alfa-romeo-racing-logo.png.transform/2col/image.png";
                    default:
                        return "000000";
                }
            }
            private string GetFlag(string nationality)
            {
                Dictionary<string, string> _flagDict = new Dictionary<string, string>()
                {
                    { "AUS", "https://ssl.gstatic.com/onebox/media/sports/logos/jSgw5z0EPOLzdUi-Aomq7Q_48x48.png" },
                    { "CAN", "https://ssl.gstatic.com/onebox/media/sports/logos/H23oIEP6qK-zNc3O8abnIA_48x48.png" },
                    { "ESP", "https://ssl.gstatic.com/onebox/media/sports/logos/5hLkf7KFHhmpaiOJQv8LmA_48x48.png" },
                    { "FIN", "https://ssl.gstatic.com/onebox/media/sports/logos/OR16mUDJv-0yTyh-jxlaKQ_48x48.png" },
                    { "FRA", "https://ssl.gstatic.com/onebox/media/sports/logos/z3JEQB3coEAGLCJBEUzQ2A_48x48.png" },
                    { "GBR", "https://ssl.gstatic.com/onebox/media/sports/logos/6HRpt1RF_AbDUftxgVUoEw_48x48.png" },
                    { "GER", "https://ssl.gstatic.com/onebox/media/sports/logos/h1FhPLmDg9AHXzhygqvVPg_48x48.png" },
                    { "ITA", "https://ssl.gstatic.com/onebox/media/sports/logos/joYpsiaYi4GDCqhSRAq5Zg_48x48.png" },
                    { "JPN", "https://ssl.gstatic.com/onebox/media/sports/logos/by4OltvtZz7taxuQtkiP3A_48x48.png" },
                    { "MEX", "https://ssl.gstatic.com/onebox/media/sports/logos/yJF9xqmUGenD8108FJbg9A_48x48.png" },
                    { "MON", "https://ssl.gstatic.com/onebox/media/sports/logos/CpHtpHDYt6p7c2iQiM324g_48x48.png" },
                    { "NED", "https://ssl.gstatic.com/onebox/media/sports/logos/8GEqzfLegwFFpe6X2BODTg_48x48.png" },
                    { "RAF", "https://ssl.gstatic.com/onebox/media/sports/logos/5Y6kOqiOIv2C1sP9C_BWtA_48x48.png" },
                };

                return _flagDict[nationality];
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

                using (WebClient client = new WebClient())
                {
                    htmlCode = client.DownloadString($"https://www.formula1.com/en/results.html/2021/drivers.html");
                }

                doc.LoadHtml(htmlCode);

                var headers = doc.DocumentNode.SelectNodes("//tr/th");
                
                List<DriverInfo> DriverStandings = new List<DriverInfo>();

                foreach (var row in doc.DocumentNode.SelectSingleNode("//table[@class='resultsarchive-table']").SelectNodes("//tr[td]"))
                {
                    if (DriverStandings.Count >= 10)
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
                    result.Add($"pos{i+1:00}", DriverStandings[i].ConcatData());
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
