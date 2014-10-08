using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTLV
{
    public class Utils
    {
        public static string ToHexStr(string Mesaj, byte[] inData, int inLen)
        {
            string output = Mesaj;

            if (inData == null)
                return output;

            if (inData.Length < inLen)
                return output;

            for (int i = 0; i < inLen; ++i)
                output = output + String.Format("{0:X02} ", inData[i]);

            return output;
        }

        public static string ToHexStr(byte[] inData, int ofset, int inLen)
        {
            string output = "";

            if (inData == null)
                return output;

            if (inData.Length < ofset + inLen)
                return output;

            for (int i = 0; i < inLen; ++i)
                output = output + String.Format("{0:X02}", inData[ofset + i]);

            return output;
        }

        public static byte[] HexToByteArray(String hexString)
        {
            hexString = hexString.Replace(" ", "");
            int NumberChars = hexString.Length;
            byte[] bytes = new byte[NumberChars / 2];

            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }

        public static byte[] BufferConcat(byte[] inData1, byte[] inData2)
        {
            List<byte> tmp = new List<byte>();
            tmp.AddRange(inData1);
            tmp.AddRange(inData2);
            return tmp.ToArray();
        }
    }
}
