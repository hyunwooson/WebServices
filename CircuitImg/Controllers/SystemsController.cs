using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CircuitImg.Controllers
{
    [ApiController]
    [Route("test")]
    public class SystemsController : ControllerBase
    {
        static SerialPort _serialPort = new SerialPort("COM4",9600);

        private readonly ILogger<SystemsController> _logger;

        public SystemsController(ILogger<SystemsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        private void Get(string interval)
        {
            string _interval = "1000";

            if (int.TryParse(interval, out int a))
                _interval = interval;

            _serialPort.Open();

            _serialPort.Write(_interval);
        }
    }
}
