using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LogingUtils
{
    public enum LogMode { FILE, CONSOLE, NONE };

    public sealed class Logger
    {
        private static readonly Logger instance = new Logger();

        private LogMode m_logMode = LogMode.NONE;

        private Logger() 
        {
            // by default logger writes to console
            m_logMode = LogMode.CONSOLE;
            Console.WriteLine("Logger Initialized....");
        }

        public static Logger Instance
        {
            get
            {
                return instance;
            }
        }

        public void setLogMode(LogMode lm) 
        {
            m_logMode = lm;
        }

        public void Log(params string[] arguments)
        {
            /*
             * Based on log mode log the information in file or dump it on console
             */

            switch (m_logMode)
            {
                case LogMode.CONSOLE:
                {
                    foreach (string s in arguments)
                    {
                        Console.Write(s);
                        Console.Write(" ");
                    }

                    Console.WriteLine();
                }
                break;

                case LogMode.FILE:
                {

                }
                break;

                case LogMode.NONE:
                {

                }
                break;
            }
        }

    }
}
