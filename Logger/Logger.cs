using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogingUtils
{
    public sealed class Logger
    {
        private static readonly Logger instance = new Logger();

        private Logger() 
        {
            Console.WriteLine("Logger Initialized....");
        }

        public static Logger Instance
        {
            get
            {
                return instance;
            }
        }

        public void Log(params string[] arguments)
        {
            foreach (string s in arguments)
            {
                Console.Write(s);
                Console.Write(" ");
            }
            Console.WriteLine();
        }
    }
}
