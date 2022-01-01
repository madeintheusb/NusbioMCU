///*
//    NusbioMatrix/NusbioPixel devices for Windows/.NET
//    MadeInTheUSB MCU ATMega328 Based Device
//    Copyright (C) 2016,2017 MadeInTheUSB LLC

//    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
//    associated documentation files (the "Software"), to deal in the Software without restriction, 
//    including without limitation the rights to use, copy, modify, merge, publish, distribute, 
//    sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
//    furnished to do so, subject to the following conditions:

//    The above copyright notice and this permission notice shall be included in all copies or substantial 
//    portions of the Software.

//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
//    LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
//    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
//    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//*/
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using MadeInTheUSB.Adafruit;
//using MadeInTheUSB.Communication;
//using MadeInTheUSB.WinUtil;
//using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
//using uint16_t = System.UInt16;
//using uint8_t = System.Byte;

//namespace MadeInTheUSB.MCU.Devices.NusbioMatrixDevice
//{
//    public partial class NusbioMatrix : Adafruit_GFX, IDisposable
//    {
        

//        static List<object> __CHAR_TABLE = new List<object>() { 

//             " " ,3, 8, "B00000000", "B00000000", "B00000000", "B00000000", "B00000000", // space
//             "!" ,1, 8, "B01011111", "B00000000", "B00000000", "B00000000", "B00000000", // !
//             "\"",3, 8, "B00000011", "B00000000", "B00000011", "B00000000", "B00000000", // "
//             "#" ,5, 8, "B00010100", "B00111110", "B00010100", "B00111110", "B00010100", // #
//             "$" ,4, 8, "B00100100", "B01101010", "B00101011", "B00010010", "B00000000", // $
//             "%" ,5, 8, "B01100011", "B00010011", "B00001000", "B01100100", "B01100011", // %
//             "&" ,5, 8, "B00110110", "B01001001", "B01010110", "B00100000", "B01010000", // &
//             "'" ,1, 8, "B00000011", "B00000000", "B00000000", "B00000000", "B00000000", // '
//             "(" ,3, 8, "B00011100", "B00100010", "B01000001", "B00000000", "B00000000", // (
//             ")" ,3, 8, "B01000001", "B00100010", "B00011100", "B00000000", "B00000000", // )
//             "*" ,5, 8, "B00101000", "B00011000", "B00001110", "B00011000", "B00101000", // *
//             "+" ,5, 8, "B00001000", "B00001000", "B00111110", "B00001000", "B00001000", // +
//             "," ,2, 8, "B10110000", "B01110000", "B00000000", "B00000000", "B00000000", // ,
//             "-" ,4, 8, "B00001000", "B00001000", "B00001000", "B00001000", "B00000000", // -
//             "." ,2, 8, "B01100000", "B01100000", "B00000000", "B00000000", "B00000000", // .
//             "/" ,4, 8, "B01100000", "B00011000", "B00000110", "B00000001", "B00000000", // /
//             "0" ,4, 8, "B00111110", "B01000001", "B01000001", "B00111110", "B00000000", // 0
//             "1" ,3, 8, "B01000010", "B01111111", "B01000000", "B00000000", "B00000000", // 1
//             "2" ,4, 8, "B01100010", "B01010001", "B01001001", "B01000110", "B00000000", // 2
//             "3" ,4, 8, "B00100010", "B01000001", "B01001001", "B00110110", "B00000000", // 3
//             "4" ,4, 8, "B00011000", "B00010100", "B00010010", "B01111111", "B00000000", // 4
//             "5" ,4, 8, "B00100111", "B01000101", "B01000101", "B00111001", "B00000000", // 5
//             "6" ,4, 8, "B00111110", "B01001001", "B01001001", "B00110000", "B00000000", // 6
//             "7" ,4, 8, "B01100001", "B00010001", "B00001001", "B00000111", "B00000000", // 7
//             "8" ,4, 8, "B00110110", "B01001001", "B01001001", "B00110110", "B00000000", // 8
//             "9" ,4, 8, "B00000110", "B01001001", "B01001001", "B00111110", "B00000000", // 9
//             ":" ,2, 8, "B01010000", "B00000000", "B00000000", "B00000000", "B00000000", // :
//             ";" ,2, 8, "B10000000", "B01010000", "B00000000", "B00000000", "B00000000", // ;
//             "<" ,3, 8, "B00010000", "B00101000", "B01000100", "B00000000", "B00000000", // <
//             "=" ,3, 8, "B00010100", "B00010100", "B00010100", "B00000000", "B00000000", // =
//             ">" ,3, 8, "B01000100", "B00101000", "B00010000", "B00000000", "B00000000", // >
//             "?" ,4, 8, "B00000010", "B01011001", "B00001001", "B00000110", "B00000000", // ?
//             "@" ,5, 8, "B00111110", "B01001001", "B01010101", "B01011101", "B00001110", // @

//             "A" ,4, 8, "B01111110", "B00010001", "B00010001", "B01111110", "B00000000", // A

//             "B" ,4, 8, "B01111111", "B01001001", "B01001001", "B00110110", "B00000000", // B
//             "C" ,4, 8, "B00111110", "B01000001", "B01000001", "B00100010", "B00000000", // C
//             "D" ,4, 8, "B01111111", "B01000001", "B01000001", "B00111110", "B00000000", // D
//             "E" ,4, 8, "B01111111", "B01001001", "B01001001", "B01000001", "B00000000", // E
//             "F" ,4, 8, "B01111111", "B00001001", "B00001001", "B00000001", "B00000000", // F
//             "G" ,4, 8, "B00111110", "B01000001", "B01001001", "B01111010", "B00000000", // G
//             "H" ,4, 8, "B01111111", "B00001000", "B00001000", "B01111111", "B00000000", // H
//             "I" ,3, 8, "B01000001", "B01111111", "B01000001", "B00000000", "B00000000", // I
//             "J" ,4, 8, "B00110000", "B01000000", "B01000001", "B00111111", "B00000000", // J
//             "K" ,4, 8, "B01111111", "B00001000", "B00010100", "B01100011", "B00000000", // K
//             "L" ,4, 8, "B01111111", "B01000000", "B01000000", "B01000000", "B00000000", // L
//             "M" ,5, 8, "B01111111", "B00000010", "B00001100", "B00000010", "B01111111", // M
//             "N" ,5, 8, "B01111111", "B00000100", "B00001000", "B00010000", "B01111111", // N
//             "O" ,4, 8, "B00111110", "B01000001", "B01000001", "B00111110", "B00000000", // O
//             "P" ,4, 8, "B01111111", "B00001001", "B00001001", "B00000110", "B00000000", // P
//             "Q" ,4, 8, "B00111110", "B01000001", "B01000001", "B10111110", "B00000000", // Q
//             "R" ,4, 8, "B01111111", "B00001001", "B00001001", "B01110110", "B00000000", // R
//             "S" ,4, 8, "B01000110", "B01001001", "B01001001", "B00110010", "B00000000", // S
//             "T" ,5, 8, "B00000001", "B00000001", "B01111111", "B00000001", "B00000001", // T
//             "U" ,4, 8, "B00111111", "B01000000", "B01000000", "B00111111", "B00000000", // U
//             "V" ,5, 8, "B00001111", "B00110000", "B01000000", "B00110000", "B00001111", // V
//             "W" ,5, 8, "B00111111", "B01000000", "B00111000", "B01000000", "B00111111", // W
//             "X" ,5, 8, "B01100011", "B00010100", "B00001000", "B00010100", "B01100011", // X
//             "Y" ,5, 8, "B00000111", "B00001000", "B01110000", "B00001000", "B00000111", // Y
//             "Z" ,4, 8, "B01100001", "B01010001", "B01001001", "B01000111", "B00000000", // Z
//             "[" ,2, 8, "B01111111", "B01000001", "B00000000", "B00000000", "B00000000", // [
//             "\\",4, 8, "B00000001", "B00000110", "B00011000", "B01100000", "B00000000", // \ "Backslash
//             "]" ,2, 8, "B01000001", "B01111111", "B00000000", "B00000000", "B00000000", // ]
//             "^" ,3, 8, "B00000010", "B00000001", "B00000010", "B00000000", "B00000000", // hat
//             "_" ,4, 8, "B01000000", "B01000000", "B01000000", "B01000000", "B00000000", // _
//             "`" ,2, 8, "B00000001", "B00000010", "B00000000", "B00000000", "B00000000", // `
//             "a" ,4, 8, "B00100000", "B01010100", "B01010100", "B01111000", "B00000000", // a
//             "b" ,4, 8, "B01111111", "B01000100", "B01000100", "B00111000", "B00000000", // b
//             "c" ,4, 8, "B00111000", "B01000100", "B01000100", "B00101000", "B00000000", // c
//             "d" ,4, 8, "B00111000", "B01000100", "B01000100", "B01111111", "B00000000", // d
//             "e" ,4, 8, "B00111000", "B01010100", "B01010100", "B00011000", "B00000000", // e
//             "f" ,3, 8, "B00000100", "B01111110", "B00000101", "B00000000", "B00000000", // f
//             "g" ,4, 8, "B10011000", "B10100100", "B10100100", "B01111000", "B00000000", // g
//             "h" ,4, 8, "B01111111", "B00000100", "B00000100", "B01111000", "B00000000", // h
//             "i" ,3, 8, "B01000100", "B01111101", "B01000000", "B00000000", "B00000000", // i
//             "j" ,4, 8, "B01000000", "B10000000", "B10000100", "B01111101", "B00000000", // j
//             "k" ,4, 8, "B01111111", "B00010000", "B00101000", "B01000100", "B00000000", // k
//             "l" ,3, 8, "B01000001", "B01111111", "B01000000", "B00000000", "B00000000", // l
//             "m" ,5, 8, "B01111100", "B00000100", "B01111100", "B00000100", "B01111000", // m
//             "n" ,4, 8, "B01111100", "B00000100", "B00000100", "B01111000", "B00000000", // n
//             "o" ,4, 8, "B00111000", "B01000100", "B01000100", "B00111000", "B00000000", // o
//             "p" ,4, 8, "B11111100", "B00100100", "B00100100", "B00011000", "B00000000", // p
//             "q" ,4, 8, "B00011000", "B00100100", "B00100100", "B11111100", "B00000000", // q
//             "r" ,4, 8, "B01111100", "B00001000", "B00000100", "B00000100", "B00000000", // r
//             "s" ,4, 8, "B01001000", "B01010100", "B01010100", "B00100100", "B00000000", // s
//             "t" ,3, 8, "B00000100", "B00111111", "B01000100", "B00000000", "B00000000", // t
//             "u" ,4, 8, "B00111100", "B01000000", "B01000000", "B01111100", "B00000000", // u
//             "v" ,5, 8, "B00011100", "B00100000", "B01000000", "B00100000", "B00011100", // v
//             "w" ,5, 8, "B00111100", "B01000000", "B00111100", "B01000000", "B00111100", // w
//             "x" ,5, 8, "B01000100", "B00101000", "B00010000", "B00101000", "B01000100", // x
//             "y" ,4, 8, "B10011100", "B10100000", "B10100000", "B01111100", "B00000000", // y
//             "z" ,3, 8, "B01100100", "B01010100", "B01001100", "B00000000", "B00000000", // z
//             "{" ,3, 8, "B00001000", "B00110110", "B01000001", "B00000000", "B00000000", // {
//             "|" ,1, 8, "B01111111", "B00000000", "B00000000", "B00000000", "B00000000", // |
//             "}" ,3, 8, "B01000001", "B00110110", "B00001000", "B00000000", "B00000000", // }
//             "~" ,4, 8, "B00001000", "B00000100", "B00001000", "B00000100", "B00000000", // ~
//            };

//        public class CharDef
//        {
//            public char Character;
//            public int ColumnCount;
//            public int Height;
//            public List<byte> Columns = new List<byte>();

//            public CharDef(char character, int columnCount, int height, params string[] columnAsBit)
//            {
//                this.Character   = character;
//                this.ColumnCount = columnCount;
//                this.Height      = height;
//                foreach (var b in columnAsBit)
//                    this.Columns.Add((byte)BitUtil.ParseBinary(b));
//            }

//            public List<bool> Reverse(int val)
//            {
//                var bVal = new List<bool>();

//                bVal.Add(BitUtil.IsSet(val, 128));
//                bVal.Add(BitUtil.IsSet(val, 64));
//                bVal.Add(BitUtil.IsSet(val, 32));
//                bVal.Add(BitUtil.IsSet(val, 16));
//                bVal.Add(BitUtil.IsSet(val, 8));
//                bVal.Add(BitUtil.IsSet(val, 4));
//                bVal.Add(BitUtil.IsSet(val, 2));
//                bVal.Add(BitUtil.IsSet(val, 1));

//                return bVal;
//            }
//        }

//        private static Dictionary<char, CharDef> _CHAR_DICTIONARY = null;

//        public static Dictionary<char, CharDef> CharDictionary
//        {
//            get
//            {
//                if (_CHAR_DICTIONARY != null)
//                    return _CHAR_DICTIONARY;

//                _CHAR_DICTIONARY = new Dictionary<char, CharDef>();
//                var i = 0;
//                while (i < __CHAR_TABLE.Count)
//                {
//                    char character = __CHAR_TABLE[i + 0].ToString()[0];
//                    var charDef = new CharDef(
//                        character,
//                        (int) __CHAR_TABLE[i + 1], // number of column to set
//                        (int) __CHAR_TABLE[i + 2], // Height always 8
//                        __CHAR_TABLE[i + 3].ToString(), 
//                        __CHAR_TABLE[i + 4].ToString(),
//                        __CHAR_TABLE[i + 5].ToString(),
//                        __CHAR_TABLE[i + 6].ToString(),
//                        __CHAR_TABLE[i + 7].ToString()
//                        );
//                    _CHAR_DICTIONARY.Add(character, charDef);
//                    i += 8;
//                }
//                return _CHAR_DICTIONARY;
//            }
//        }

//        public void WriteChar_v0(int deviceIndex, char character, bool clear = true, int x = 2, int y = 0)
//        {
//            if (!CharDictionary.ContainsKey(character))
//                throw new ArgumentException(string.Format("Character '{0}' is not defined in CharDictionary"));

//            if(clear)
//                this.Clear(maxtrixIndex: deviceIndex);

//            var charDef = CharDictionary[character];

//            for (int i = 0; i < charDef.ColumnCount; i++)
//            {
//                int c = x + i;
//                if (c >= 0 && c < 80)
//                {
//                    var dots = charDef.Reverse(charDef.Columns[i]);
//                    for (var r = 0; r < NusbioMatrix.MATRIX_ROW_SIZE; r++)
//                    {
//                        this.SetLed(deviceIndex, c, r, dots[r]);
//                    }
//                }
//            }
//        }

//        public void WriteChar_v1(int deviceIndex, char character, bool clear = true, int x = 2, int y = 0)
//        {
//            if (!CharDictionary.ContainsKey(character))
//                throw new ArgumentException(string.Format("Character '{0}' is not defined in CharDictionary"));

//            if(clear)
//                this.Clear(maxtrixIndex: deviceIndex);

//            var charDef = CharDictionary[character];

//            // Compute the character in temp 8 byte array
//            byte[] pixels = new byte[MATRIX_ROW_SIZE];
//            for (int i = 0; i < charDef.ColumnCount; i++)
//            {
//                int c = x + i;
//                if (c >= 0 && c < 80)
//                {
//                    var dots = charDef.Reverse(charDef.Columns[i]);
//                    for (var r = 0; r < NusbioMatrix.MATRIX_ROW_SIZE; r++)
//                    {
//                        SetLedMemory(pixels, c, r + y, dots[r]);
//                    }
//                }
//            }
//            for (var r = 0; r < MATRIX_ROW_SIZE; r++)
//            {
//                this._pixels[(MATRIX_ROW_SIZE * deviceIndex) + r] = pixels[r];
//            }

//            var sb = new StringBuilder();
//            sb.AppendFormat("CharDefinitions['{0}'] = new CharDefinition('{0}', ", character);
//            sb.AppendFormat("new List<string>() {{");
//            for (var r = MATRIX_ROW_SIZE-1; r >= 0; r--)
//            {
//                sb.AppendFormat(@"""B{0}""", BitUtil.BitRpr(pixels[r]));
//                if (r > 0) sb.Append(", ");
//            }
//            sb.AppendFormat(" }} );").AppendLine();
//            System.IO.File.AppendAllText(@"d:\chars.cs", sb.ToString());
//        }
        
//        private byte[] SetLedMemory(byte[] pixels, int column, int row, bool state)
//        {
//            var offset = 0;
//            //val = (byte)(128 >> column);
//            byte val = (byte)(1 << column);
//            if(state)
//                pixels[offset + row] = (byte)(pixels[offset + row] | val);
//            else {
//                val = (byte)(~val);
//                pixels[offset + row] = (byte)(pixels[offset + row] & val);
//            }
//            return _pixels;
//        }
//    }
//}
