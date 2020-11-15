using System;
using System.Text;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var loc = "CA,United states";

            char middleChar = Convert.ToChar(65 + (loc.Length > 31 ? loc.Length - 6 : loc.Length));

            var uule = "w+CAIQICI" + middleChar + Convert.ToBase64String(Encoding.UTF8.GetBytes(loc));
        }
    }
}
