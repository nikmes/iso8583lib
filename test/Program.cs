using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iso8583;
using ConvertionsHelper;
using LogingUtils;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {

            Logger.Instance.Log("Demo program to build an ISO8583 message");
            
            iso8583.isoMessage iMsg = new isoMessage(null, 0);

            iMsg.msgDialect.traceToFile();

            iMsg.msgHeader.setHeader("360000200000");

            iMsg.setFieldValue(000, "0800");
            iMsg.setFieldValue(003, "300000");
            iMsg.setFieldValue(039, "00");
            iMsg.setFieldValue(067, "12345678");
            iMsg.packForTransmission();

            Logger.Instance.Log("ISO Bitmap HEX:     [" + iMsg.getBitmapHex() + "]");         
            Logger.Instance.Log("ISO Bitmap Binary:  [" + iMsg.getBitmapBin() + "]");
            Logger.Instance.Log("Message Buffer HEX: [" + iMsg.getBufferHex() + "]");

            iMsg.msgDialect.saveToFile("dialects\\dialect.xml");
        }
    }
}
