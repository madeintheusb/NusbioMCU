/*
    Copyright (C) 2016,2017 MadeInTheUSB LLC

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
    associated documentation files (the "Software"), to deal in the Software without restriction, 
    including without limitation the rights to use, copy, modify, merge, publish, distribute, 
    sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial 
    portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
    LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MadeInTheUSB.WinUtil
{
    public class BitUtil {

        public static List<int> pof2 = new List<int>()  { 1, 2, 4, 8, 16, 32, 64, 128};

        public static byte Inverse(byte v)
        {
            //byte v = (byte)BitUtil.ParseBinary("B11011101"); 
            var newV = 0;
            var j    = 0;
            for (var i = 8 - 1; i >= 0; i--)
            {
                var isSet = BitUtil.IsSet(v, (byte) pof2[i]);
                if (isSet)
                    newV += pof2[j];
                j++;
            }
            return (byte)newV;
        }

        public static List<int> Range(int start, int end) {

            return Enumerable.Range(start, end).ToList();
        }

        public static List<int> ParseBinary(List<string> binaryValues)
        {
            var l = new List<int>();
            foreach (var bv in binaryValues)
                l.Add(ParseBinary(bv));
            return l;
        }

        public static int ParseBinary(string s)
        {
            if (s.ToUpperInvariant().StartsWith("B"))
            {
                return Convert.ToInt32(s.Substring(1), 2);
            }
            else throw new ArgumentException(string.Format("Invalid binary value:{0}", s));
        }

        public static string BitRpr(byte [] buffer, bool newLine = true)
        {
            var t = new StringBuilder(1024);

            for (var i = 0; i < buffer.Length; i++) {

                t.AppendFormat("[{0}] {1}:{2} ", i.ToString("000"), buffer[i].ToString("000"), WinUtil.BitUtil.BitRpr(buffer[i]));
                if (newLine)
                    t.AppendLine();
            }
            return t.ToString();
        }

        public static string BitRpr(byte value, bool detail = false)
        {
            if (detail)
            {
                return string.Format("{0}:{1}", value.ToString("000"), BitRpr(value, false));
            }
            else
            {
                var s = System.Convert.ToString(value, 2);
                return s.PadLeft(8, '0');
            }
        }

        public static string BitRpr(int value)
        {
            var s = System.Convert.ToString(value, 2);
            return s.PadLeft(8, '0');
            return s;
        }

        public static byte UnsetBit(byte value, byte bit)
        {
            value &= ((byte) ~ bit);
            //if ((value & bit) == bit)
            //    value -= bit;
            return value;
        }


        public static ushort Byte2UInt16(byte high_byte, byte low_byte)
        {
            int a = ((high_byte << 8)) | (low_byte);
            return (ushort)a;
        }
        
        public static List<byte> SliceBuffer(List<byte> buffer, int start, int count)
        {
            var l = new List<byte>();
            for (var i = start; i < start + count; i++)
            {
                l.Add(buffer[i]);
            }
            return l;
        }

        public static List<byte> ByteBuffer(params  int [] integers)
        {
            var l = new List<byte>(integers.Length);
            foreach (var i in integers)
                l.Add((byte)i);
            return l;
        }

        public static byte HighByte(ushort number)
        {
            byte upper = (byte) (number >> 8);            
            return upper;
        }

        public static byte LowByte(ushort number)
        {
            byte lower = (byte) (number & 0xff);
            return lower;
        }
        
        public static bool IsSet(int value, byte bit)
        {
            return IsSet((byte) value, bit);
        }

        public static bool IsSet(byte value, byte bit)
        {
            if(value == 0 && bit ==0)
                return false;
            return (value & bit) == bit;
        }

        public static byte UnsetBitByIndex(byte value, int bitIndex)
        {
            var bit = (byte)Math.Pow(2, bitIndex);
            return UnsetBit(value, bit);

            //if ((value & bit) == bit)
            //    value -= bit;
            //return value;
        }

        public static byte SetBitIndex(byte value, int bitIndex)
        {
            var bit = (byte)Math.Pow(2, bitIndex);
            return SetBit(value, bit);

            //if ((value & bit) != bit)
            //    value += bit;
            //return value;
        }

        public static byte SetBit(byte value, byte bit)
        {
            value |= bit;
            //if ((value & bit) != bit)
            //    value += bit;
            return value;
        }

        public static byte SetBitOnOff(int value, byte bit, bool on)
        {
            if (on)
                return SetBit((byte)value, bit);
            else
                return UnsetBit((byte)value, bit);
        }
    }

  
}


