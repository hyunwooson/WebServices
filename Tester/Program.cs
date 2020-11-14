using System;


namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var weather = new WeatherNet.Clients.CurrentWeather().GetByCityName("Seoul", "Korea");
            var ss = weather.Message;

        }
    }
}
