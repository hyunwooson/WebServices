﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public string Get(string shift)
        {
            string gameTitle = "";
            string gameTime = "";
            string awayScore = "";
            string awayTeam = "";
            string homeScore = "";
            string homeTeam = "";

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
                    htmlCode = client.DownloadString($"https://www.google.com/search?q={searchText.Replace(' ', '+')}&hl=en");
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

                    break;
                }
            }

            if (targetNode == null)
            {
                return new JObject()
                {
                    {"vs", "vs."},
                    { "gameTitle", gameTitle },
                    { "gameTime" , gameTime  },
                    { "oppScore", awayScore },
                    { "oppTeam" , awayTeam  },
                    { "bearsScore", homeScore },
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


            return new JObject()
            {
                { "vs" , home? "vs" : "@"},
                { "gameTitle", gameTitle },
                { "gameTime" , gameTime  },
                { "oppScore", home? awayScore : homeScore },
                { "oppTeam" , home? awayTeam : homeTeam  },
                { "bearsScore" , home? homeScore : awayScore },
                { "title" , title}
            }.ToString();
        }
    }
}


