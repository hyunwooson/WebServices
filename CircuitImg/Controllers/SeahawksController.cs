using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Ical.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace WebServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeahawksController : ControllerBase
    {
        class Event
        {
            public string vs = "";
            public string opp = "";
            public DateTime eventTimeUTC = new DateTime();
        }
        [HttpGet]
        public string Get(string shift)
        {
      
            if (!Int32.TryParse(shift, out int timeShift))
            {
                timeShift = 0;
            }

            string vs = "";
            string opp = "";
            string date = "";
            string time = "";

            string text = "";

            var webRequest = WebRequest.Create(@"http://cal.events/CUSHfZ.ics");

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                text = reader.ReadToEnd();
            }
            var calendar = Calendar.Load(text);
            
            List<Event> eventList = new List<Event>();

            foreach (var evnt in calendar.Events)
            {
                var _evnt = evnt.Summary.Split(' ');
                int i = evnt.Summary.Contains("Preseason") ? 2 : 1;
                eventList.Add(new Event()
                {
                    vs= _evnt[i].Equals("at")? "@":"vs",
                    opp = _evnt[i+1],
                    eventTimeUTC = evnt.Start.AsUtc
                }) ;
            }

            eventList = eventList.OrderBy(a => a.eventTimeUTC).ToList();



            var evntTime = new DateTime();

            for (int i = 0; i < eventList.Count; i++)
            {
                if (DateTime.UtcNow < eventList[i].eventTimeUTC)
                {
                    vs = eventList[i].vs;
                    opp = eventList[i].opp;
                    evntTime = eventList[i].eventTimeUTC;
                    break;
                }
            }

            if (evntTime.AddSeconds(timeShift).Date == DateTime.UtcNow.AddSeconds(timeShift).Date)
                date = "Today";
            else if (evntTime.AddSeconds(timeShift).Date == DateTime.UtcNow.AddDays(1).AddSeconds(timeShift).Date)
                date = "Tomorrow";
            else
                date = evntTime.AddSeconds(timeShift).ToString("MMM. d, yyyy");

            time = evntTime.AddSeconds(timeShift).ToString("HH:mm");


            return new JObject()
            {
                { "date", date },
                { "time", time },
                { "vs", vs },
                { "opp",opp }
            }.ToString();
        }

        [HttpGet("standings")]
        public string GetStandings()
        {
            JObject rslt = new JObject();

            try
            {
                HtmlDocument doc = new HtmlDocument();
                HtmlNode targetNode = null;

                string htmlCode = "";

                using (WebClient client = new WebClient())
                {
                    htmlCode = client.DownloadString($"https://www.nfl.com/standings/");
                }

                doc.LoadHtml(htmlCode);



                foreach (var _node in doc.DocumentNode.SelectNodes("//table[@summary='Standings - Detailed View']"))
                {
                    if (_node.InnerText.Contains("NFC WEST"))
                    {
                        doc = new HtmlDocument();
                        doc.LoadHtml(_node.InnerHtml);
                        break;
                    }
                }

                int i = 0;

                var nodes = doc.DocumentNode.SelectNodes("//tr[td]");
                foreach (var row in nodes)
                {
                    var _arr = row.SelectNodes("td").Select(td => td.InnerText).ToArray();

                    string team = _arr[0].Split("\n").Where(_s => !_s.Trim().StartsWith("x") && _s.Trim().Length > 1).ToArray().Last().Trim();
                    string _s = "";

                    _s += team + ";";
                    _s += _arr[1].Trim() + ";";
                    _s += _arr[2].Trim() + ";";
                    _s += _arr[3].Trim() + ";";
                    _s += _arr[15].Trim() + ";";

                    rslt.Add($"pos{++i}", _s);
                }

            }
            catch (Exception)
            {
                rslt = new JObject()
                {
                    {"pos1","ERR;0;0;0;W0;" },
                    {"pos2","ERR;0;0;0;W0;" },
                    {"pos3","ERR;0;0;0;W0;" },
                    {"pos4","ERR;0;0;0;W0;" },
                };
            }

            return rslt.ToString();
        }
    }
}
