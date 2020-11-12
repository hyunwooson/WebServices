using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CircuitImg.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Controller : ControllerBase
    {
        static SerialPort _serialPort = new SerialPort("COM4", 9600);

        [HttpGet]
        public string Get(string interval)
        {
            string _interval = "1000";

            if (int.TryParse(interval, out int a))
                _interval = interval;

            if (!_serialPort.IsOpen)
                _serialPort.Open();
            var buffer = Encoding.UTF8.GetBytes(_interval);
            _serialPort.Write(buffer,0,buffer.Length);
            _serialPort.Close();
            return "Hello World";
        }
    }
}
