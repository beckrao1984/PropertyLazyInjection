using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyLazyInjection
{
    public class Logger : ILogger
    {
        public Logger()
        {
            Console.WriteLine("Logger is created");
        }
    }
}
