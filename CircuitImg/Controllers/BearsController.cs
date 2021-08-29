using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace WebServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BearsController : ControllerBase
    {
        [HttpGet()]
        public string Get(string loc)
        {
            string gameTitle = "";
            string gameTime = "";
            string awayScore = "";
            string awayTeam = "";
            string homeScore = "";
            string homeTeam = "";

            //var loc = "CA,United states";

            char middleChar = Convert.ToChar(65 + (loc.Length > 31 ? loc.Length - 6 : loc.Length));

            var uule = "w+CAIQICI" + middleChar + Convert.ToBase64String(Encoding.UTF8.GetBytes(loc));


            HtmlDocument doc = new HtmlDocument();
            HtmlNode targetNode = null;

            string[] variations = new string[] { "", " next game", " last game" };

            for (int i = 0; i < variations.Length; i++)
            {
                string htmlCode = "";

                string team = "doosan bears";
                //string team = "blackpool";
                //string team = "la dodgers";
                //string team = "new york knicks";
                //string team = "guangzhou hengda";
                //string team = "FC Seoul";
                //string team = "formula 1";
                //string team = "lg twins";
                //string team = "lotte giants";

                string searchText = team + variations[i];

                using (WebClient client = new WebClient())
                {
                    htmlCode = client.DownloadString($"https://www.google.com/search?q={searchText.Replace(' ', '+')}&hl=en&uule={uule}");
                }

                doc.LoadHtml(htmlCode);
                targetNode = doc.DocumentNode.SelectSingleNode("//div[@class='zGNVZb']");

                if (targetNode != null)
                {
                    var strTG = targetNode.ParentNode.InnerHtml;

                    doc.LoadHtml(strTG);

                    targetNode = doc.DocumentNode;

                    var gameTimeNode = targetNode.SelectSingleNode("//div[@class='BNeawe s3v9rd AP7Wnd lRVwie']");
                    gameTime = "";
                    if (gameTimeNode != null)
                        gameTime = targetNode.SelectSingleNode("//div[@class='BNeawe s3v9rd AP7Wnd lRVwie']").InnerText;

                    if (gameTime.ToUpper().Contains("YESTERDAY"))
                        continue;

                    if(i == 0 && gameTime.ToUpper().Contains("FINAL"))
                    {
                        var _gtArray = gameTime.Split(',');
                        bool continueFlag = false;
                        foreach (var item in _gtArray)
                        {
                            if (DateTime.TryParse(item.Trim(), out DateTime dt) && dt< DateTime.Today)
                            {
                                continueFlag = true;
                                break;
                            }
                        }
                        if (continueFlag)
                            continue;
                    }

                    break;
                }
            }



            if (targetNode == null)
            {
                return new JObject()
                {
                    { "vs" , ""},
                    { "gameTitle", "" },
                    { "gameTime" , ""  },
                    { "oppScore", "" },
                    { "oppTeam" , ""  },
                    { "bearsScore" , "" },
                    { "title" , "" },
                }.ToString();
            }

            var gameTitleNode = targetNode.SelectSingleNode("//div[@class='BNeawe tAd8D AP7Wnd']");
            gameTitle = "";
            if (gameTitleNode != null)
                gameTitle = targetNode.SelectSingleNode("//div[@class='BNeawe tAd8D AP7Wnd']").InnerText.Replace(" � ", "/");

            var awayInfoNode = targetNode.SelectSingleNode("//div[@class='AP66Yc Q38Sd']");
            string awayInfo = "";
            if (awayInfoNode != null)
                awayInfo = targetNode.SelectSingleNode("//div[@class='AP66Yc Q38Sd']").InnerText;
            awayTeam = "";
            awayScore = "";
            foreach (var _ch in awayInfo)
            {
                if (int.TryParse(_ch.ToString(), out int a))
                    awayScore += _ch;
                else
                    awayTeam += _ch;
            }

            var homeInfoNode = targetNode.SelectSingleNode("//div[@class='AP66Yc']");
            string homeInfo = "";
            if (homeInfoNode != null)
                homeInfo = targetNode.SelectSingleNode("//div[@class='AP66Yc']").InnerText;
            homeTeam = "";
            homeScore = "";
            foreach (var _ch in homeInfo)
            {
                if (int.TryParse(_ch.ToString(), out int a))
                    homeScore += _ch;
                else
                    homeTeam += _ch;
            }

            bool home = homeTeam.Equals("Doosan");

            awayScore = awayScore.Equals("") ? awayTeam: awayScore;
            homeScore = homeScore.Equals("") ? homeTeam: homeScore;

            string title = gameTitle.ToLower().Contains("korean") ? "ks" : gameTitle.Length < 5 ? "kbo" : "ps";

            if (gameTime.ToUpper().Contains("FINAL"))
            {
                var _gtArray = gameTime.Split(',');
                foreach (var item in _gtArray)
                {
                    if (DateTime.TryParse(item.Trim(), out DateTime dt)
                        && gameTitle.Contains("Korean Series"))
                    {
                        return new JObject()
                        {
                            { "vs" , "vs"},
                            { "gameTitle", "Season Ended" },
                            { "gameTime" , ""  },
                            { "oppScore", "" },
                            { "oppTeam" , "TBA"  },
                            { "bearsScore" , "Bears" },
                            { "title" , "kbo" },
                        }.ToString();
                    }
                }
            }

            return new JObject()
            {
                { "vs" , home? "vs" : "@"},
                { "gameTitle", gameTitle },
                { "gameTime" , gameTime  },
                { "oppScore", home? awayScore : homeScore },
                { "oppTeam" , home? awayTeam : homeTeam  },
                { "bearsScore" , home? homeScore : awayScore },
                { "title" , title},
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
                    htmlCode = client.DownloadString($"http://eng.koreabaseball.com/Standings/TeamStandings.aspx?searchDate=2021-12-31");
                }

                doc.LoadHtml(htmlCode);

                var headers = doc.DocumentNode.SelectNodes("//tr/th");

                int i = 0;
                foreach (var row in doc.DocumentNode.SelectSingleNode("//table[@summary='team standings']").SelectNodes("//tr[td]"))
                {
                    if (i >= 10)
                        break;
                    var _arr = row.SelectNodes("td").Select(td => td.InnerText).ToArray();
                    string _s = "";
                    _s += _arr[0] + ";";
                    _s += _arr[1] + ";";
                    _s += _arr[3] + ";";
                    _s += _arr[4] + ";";
                    _s += _arr[5] + ";";
                    _s += _arr[7] + ";";
                    rslt.Add($"pos{++i}", _s);
                }
            }
            catch (Exception)
            {
                rslt = new JObject()
                {
                    { "pos1" , "1;ERR;00;00;00;00.0;" },
                    { "pos2" , "2;ERR;00;00;00;00.0;" },
                    { "pos3" , "3;ERR;00;00;00;00.0;" },
                    { "pos4" , "4;ERR;00;00;00;00.0;" },
                    { "pos5" , "5;ERR;00;00;00;00.0;" },
                    { "pos6" , "6;ERR;00;00;00;00.0;" },
                    { "pos7" , "7;ERR;00;00;00;00.0;" },
                    { "pos8" , "8;ERR;00;00;00;00.0;" },
                    { "pos9" , "9;ERR;00;00;00;00.0;" },
                    { "pos10" , "10;ERR;00;00;00;00.0;" },
                };

            }

            return rslt.ToString();
        }
    }
}


