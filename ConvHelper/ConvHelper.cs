using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ConvertionsHelper
{
    public static class ConvHelper
    {
        public static string bin2hex(string binary)
        {
            StringBuilder result = new StringBuilder(binary.Length / 8 + 1);

            // TODO: check all 1's or 0's... Will throw otherwise

            int mod4Len = binary.Length % 8;
            if (mod4Len != 0)
            {
                // pad to length multiple of 8
                binary = binary.PadLeft(((binary.Length / 8) + 1) * 8, '0');
            }

            for (int i = 0; i < binary.Length; i += 8)
            {
                string eightBits = binary.Substring(i, 8);
                result.AppendFormat("{0:X2}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString();
        }

        public static string hex2bin(string hexstring)
        {

            String binarystring = String.Join(String.Empty, hexstring.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));

            return binarystring;
        }

        public static string bytes2hex(byte[] ba, int len, int start)
        {
            /*
             * Converts a byte array to hexadecimal string
             */

            Regex rgx = new Regex("-");

            string hex = BitConverter.ToString(ba, start, len);

            hex = rgx.Replace(hex, "");

            return (hex);
        }

        public static string bytes2hexWs(byte[] ba, int len, int start)
        {
            /*
             * Converts a byte array to hexadecimal string - hex charactres are seperated by space
             */

            Regex rgx = new Regex("-");

            string hex = BitConverter.ToString(ba, start, len);

            hex = rgx.Replace(hex, " ");

            return (hex);
        }

        public static byte[] hex2bytes(string hex)
        {
            /*
             * Converts a hexadecimal string to byte array
             */

            return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }

        public static string hex2ascii(string hex)
        {
            /*
             * Converts a hex string to ascii
             */

            string res = String.Empty;

            for (int a = 0; a < hex.Length; a = a + 2)
            {
                string Char2Convert = hex.Substring(a, 2);
                int n = Convert.ToInt32(Char2Convert, 16);
                char c = (char)n;
                res += c.ToString();
            }

            return res;
        }

        public static string ascii2hex(string ascii)
        {
            /*
             * Converts am ascii string to hex
             */

            StringBuilder sb = new StringBuilder();
            byte[] inputBytes = Encoding.UTF8.GetBytes(ascii);

            foreach (byte b in inputBytes)
            {
                sb.Append(string.Format("{0:x2}", b));
            }

            return sb.ToString();
        }

        public static string ebcdic2ascii()
        {
            /*
             * Converts an asxii string to ebcdic
             */
            return String.Empty;
        }

        public static string ascii2ebcdic()
        {
            /*
             * Converts an ebcdic string to ascii
             */
            return String.Empty;
        }

        public static ulong byte2int(byte[] array)
        {
            int pos = 0;
            ulong result = 0;
            foreach (byte by in array)
            {
                result |= (ulong)(by << pos);
                pos += 8;
            }
            return result;
        }
    }
}
