using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
                eventList.Add(new Event() 
                {
                    vs= evnt.Summary.Split(' ')[1].Equals("at")? "@":"vs",
                    opp = evnt.Summary.Split(' ')[2],
                    eventTimeUTC = evnt.Start.AsUtc
                });
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
    }
}
