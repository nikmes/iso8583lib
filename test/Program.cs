using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iso8583;
using ConvertionsHelper;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            iso8583.isoMessage iMsg = new isoMessage(null, 0);

            iMsg.msgDialect.traceToFile();

            iMsg.msgHeader.setHeader("360000200000");

            iMsg.setFieldValue(000, "0800");
            iMsg.setFieldValue(003, "300000");
            iMsg.setFieldValue(039, "00");

            iMsg.packForTransmission();

            Console.WriteLine("ISO Bitmap HEX:     [" + iMsg.getBitmapHex() + "]");
             
            Console.WriteLine("ISO Bitmap Binary:  [" + iMsg.getBitmapBin() + "]");

            Console.WriteLine("Message Buffer HEX: [" + iMsg.getBufferHex() + "]");

            iMsg.msgDialect.saveToFile("dialect.xml");
        }
    }
}
