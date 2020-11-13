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
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        [HttpGet]
        [Route("image/kbo/emblems")]
        public ActionResult Getimage(string team)
        {
            if (team == null)
                team = "";
            switch (team.ToLower())
            {
                case "doosan":
                    return File(WebServices.Properties.Resource.doosan, "image/png");
                case "nc":
                    return File(WebServices.Properties.Resource.nc, "image/png");
                case "kt":
                    return File(WebServices.Properties.Resource.kt, "image/png");
                case "samsung":
                    return File(WebServices.Properties.Resource.samsung, "image/png");
                case "lotte":
                    return File(WebServices.Properties.Resource.lotte, "image/png");
                case "hanwha":
                    return File(WebServices.Properties.Resource.hanwha, "image/png");
                case "lg":
                    return File(WebServices.Properties.Resource.lg, "image/png");
                case "kia":
                    return File(WebServices.Properties.Resource.kia, "image/png");
                case "kiwoom":
                    return File(WebServices.Properties.Resource.kiwoom, "image/png");
                case "sk":
                    return File(WebServices.Properties.Resource.sk, "image/png");


                default:
                    return File(WebServices.Properties.Resource.notfound, "image/png");
            }
        }
    }
}
