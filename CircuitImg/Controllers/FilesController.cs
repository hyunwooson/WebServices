using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebServices.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        #region KBO
        [HttpGet]
        [Route("kbo/emblems")]
        public ActionResult GetKBOteamimage(string team)
        {
            if (team == null)
                team = "";

            var target = Properties.KBO.Resource.ResourceManager.GetObject(team.ToLower());

            if (target != null)
                return File((byte[])target, "image/png");
            else
                return File(Properties.Common.Resource.notfound, "image/png");
        }

        [HttpGet]
        [Route("kbo/title")]
        public ActionResult GetKBOTitle(string title)
        {
            if (title == null)
                title = "";

            var target = Properties.KBO.Resource.ResourceManager.GetObject(title.ToLower());

            if (target != null)
                return File((byte[])target, "image/png");
            else
                return File(Properties.Common.Resource.notfound, "image/png");
        }

        #endregion

        #region NFL
        [HttpGet]
        [Route("nfl/emblems")]
        public ActionResult GetNFLteamimage(string team)
        {
            if (team == null)
                team = "";
            else if (int.TryParse(team[0].ToString(), out int a))
                team = "_" + team;

            var target = Properties.NFL.Resource.ResourceManager.GetObject(team.ToLower());

            if (target != null)
                return File((byte[])target, "image/png");
            else
                return File(Properties.Common.Resource.notfound, "image/png");
        }
        #endregion

        #region Weather
        [HttpGet]
        [Route("weather")]
        public ActionResult GetWeatherIcon(string code)
        {
            if (code == null)
                code = "";

            else if (int.TryParse(code[0].ToString(), out int a))
                code = "_" + code;

            var target = Properties.Weather.Resource.ResourceManager.GetObject(code);

            if (target == null)
            {
                target = Properties.Weather.Resource.ResourceManager.GetObject(code.Replace("_n",""));
            }

            if (target == null)
            {
                target = Properties.Weather.Resource.ResourceManager.GetObject(code.Substring(0,2)+"00");
            }

            if (target != null)
                return File((byte[])target, "image/png");
            else
                return File(Properties.Common.Resource.notfound, "image/png");
        }
        #endregion

    }
}
