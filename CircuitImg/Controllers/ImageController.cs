using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace WebServices.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        string imageFolder = @"D:\images\";


        [HttpGet]
        [Route("cddb")]
        public ActionResult GetAlbumimage(string album, string artist)
        {
            var target = System.IO.File.ReadAllBytes(imageFolder + @"cddb\" + artist + "_" + album + ".png");

            if (target != null)
                return File((byte[])target, "image/png");
            else
                return File(Properties.Common.Resource.notfound, "image/png");
        }
    }
}
