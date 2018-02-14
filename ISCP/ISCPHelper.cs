using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AppOnkyo.ISCP
{
    class ISCPHelper
    {
        public static string[] Parse(string raw)
        {
            raw = raw.Substring(raw.IndexOf("!") + 2);
            raw = raw.Substring(0, raw.Length - 1);
            var r = new[] {raw.Substring(0, 3), raw.Substring(3)};
            Log.Debug("ISCPParser:", $"{r[0]}{r[1]}");
            return r;
        }

        public static byte[] Generate(string cmd,string h)
        {
            int length = cmd.Length + 3;
            if (h == "x")
                length++;
            var hex = $"{(char)length}";
            string l = $"ISCP\0\0\0\u0010\0\0\0{hex}\u0001\0\0\0!{h}{cmd}\r\n";
            byte[] bts = Encoding.ASCII.GetBytes(l);
            return bts;
        }

        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
                throw new ArgumentNullException("hexString");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("hexString must have an even length", "hexString");
            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return bytes;
        }
    }
}