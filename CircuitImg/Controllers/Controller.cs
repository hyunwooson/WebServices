using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CircuitImg.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Controller : ControllerBase
    {

        [HttpGet]
        public string Get()
        {
            return "Hello World";
        }
    }
}
