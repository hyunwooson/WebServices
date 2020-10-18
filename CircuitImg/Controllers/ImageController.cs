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

namespace CircuitImg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        

        private readonly ILogger<ImageController> _logger;

        public ImageController(ILogger<ImageController> logger)
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

            string url = "";
            string flag = "";
            string title = "";
            string date = "";
            string time = "";
            string session = "";
            string circuit = "";

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
                    break;
                }
            }

            title = evntName.Substring(0, evntName.IndexOf("2020") - 1);
            session = evntName.Substring(evntName.IndexOf("-") + 2);
            date = evntTime.AddSeconds(timeShift).ToString("MMM. d, yyyy");
            time = evntTime.AddSeconds(timeShift).ToString("HH:mm");


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

            return new JObject()
            {
                { "title", title },
                { "session", session },
                { "date", date },
                { "time", time},
                { "circuit", circuit},
                { "url", url },
                { "flag", flag }
            }.ToString();
        }
        
        [HttpGet("test")]
        public string GetTest([FromQuery]string shift)
        {
            return "Passed parameter is \'" + shift + "\'";
        }
    }
}
