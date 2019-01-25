/*
    NusbioMatrix/NusbioPixel devices for Windows/.NET
    MadeInTheUSB MCU ATMega328 Based Device
    Copyright (C) 2016,2017 MadeInTheUSB LLC 

    MIT license, all text above must be included in any redistribution

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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadeInTheUSB.Adafruit;
using MadeInTheUSB.Communication;
using MadeInTheUSB.WinUtil;
using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t = System.Byte;

namespace MadeInTheUSB.MCU
{
    public partial class NusbioMatrix : IDisposable
    {
        public class CharDefinition
        {
            public char Char;
            public List<byte> Bitmap = new List<byte>();

            public CharDefinition(char character, List<string> bitmap)
            {
                this.Char = character;
                for (var i = 0; i < NusbioMatrix.MATRIX_ROW_SIZE; i++)
                    this.Bitmap.Add((byte) BitUtil.ParseBinary(bitmap[i]));
            }
        }

        private static Dictionary<char, CharDefinition> __CHAR_DIC;

        public static Dictionary<char, CharDefinition> CHAR_DIC
        {
            get
            {
                if (__CHAR_DIC == null)
                {
                    __CHAR_DIC = new Dictionary<char, CharDefinition>();

                    __CHAR_DIC[' '] = new CharDefinition(' ', new List<string>() {"B00000000", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000" } );
                    __CHAR_DIC['!'] = new CharDefinition('!', new List<string>() {"B00000100", "B00000100", "B00000100", "B00000100", "B00000100", "B00000000", "B00000100", "B00000000" } );
                    __CHAR_DIC['"'] = new CharDefinition('"', new List<string>() {"B00010100", "B00010100", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000" } );
                    __CHAR_DIC['#'] = new CharDefinition('#', new List<string>() {"B00000000", "B00101000", "B01111100", "B00101000", "B01111100", "B00101000", "B00000000", "B00000000" } );
                    __CHAR_DIC['$'] = new CharDefinition('$', new List<string>() {"B00010000", "B00111000", "B00000100", "B00011000", "B00100000", "B00011100", "B00001000", "B00000000" } );
                    __CHAR_DIC['%'] = new CharDefinition('%', new List<string>() {"B01001100", "B01001100", "B00100000", "B00010000", "B00001000", "B01100100", "B01100100", "B00000000" } );
                    __CHAR_DIC['&'] = new CharDefinition('&', new List<string>() {"B00001000", "B00010100", "B00010100", "B00001000", "B01010100", "B00100100", "B01011000", "B00000000" } );
                    __CHAR_DIC['\'']= new CharDefinition('\'',new List<string>() {"B00000100", "B00000100", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000" } );
                    __CHAR_DIC['('] = new CharDefinition('(', new List<string>() {"B00010000", "B00001000", "B00000100", "B00000100", "B00000100", "B00001000", "B00010000", "B00000000" } );
                    __CHAR_DIC[')'] = new CharDefinition(')', new List<string>() {"B00000100", "B00001000", "B00010000", "B00010000", "B00010000", "B00001000", "B00000100", "B00000000" } );
                    __CHAR_DIC['*'] = new CharDefinition('*', new List<string>() {"B00000000", "B00010000", "B00010000", "B01111100", "B00101000", "B01000100", "B00000000", "B00000000" } );
                    __CHAR_DIC['+'] = new CharDefinition('+', new List<string>() {"B00000000", "B00010000", "B00010000", "B01111100", "B00010000", "B00010000", "B00000000", "B00000000" } );
                    __CHAR_DIC[','] = new CharDefinition(',', new List<string>() {"B00000000", "B00000000", "B00000000", "B00000000", "B00001100", "B00001100", "B00001000", "B00000100" } );
                    __CHAR_DIC['-'] = new CharDefinition('-', new List<string>() {"B00000000", "B00000000", "B00000000", "B00111100", "B00000000", "B00000000", "B00000000", "B00000000" } );
                    __CHAR_DIC['.'] = new CharDefinition('.', new List<string>() {"B00000000", "B00000000", "B00000000", "B00000000", "B00000000", "B00001100", "B00001100", "B00000000" } );
                    __CHAR_DIC['/'] = new CharDefinition('/', new List<string>() {"B00100000", "B00010000", "B00010000", "B00001000", "B00001000", "B00000100", "B00000100", "B00000000" } );
                    __CHAR_DIC['0'] = new CharDefinition('0', new List<string>() {"B00011000", "B00100100", "B00100100", "B00100100", "B00100100", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['1'] = new CharDefinition('1', new List<string>() {"B00001000", "B00001100", "B00001000", "B00001000", "B00001000", "B00001000", "B00011100", "B00000000" } );
                    __CHAR_DIC['2'] = new CharDefinition('2', new List<string>() {"B00011000", "B00100100", "B00100000", "B00010000", "B00001000", "B00000100", "B00111100", "B00000000" } );
                    __CHAR_DIC['3'] = new CharDefinition('3', new List<string>() {"B00011000", "B00100100", "B00100000", "B00010000", "B00100000", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['4'] = new CharDefinition('4', new List<string>() {"B00100000", "B00110000", "B00101000", "B00100100", "B00111100", "B00100000", "B00100000", "B00000000" } );
                    __CHAR_DIC['5'] = new CharDefinition('5', new List<string>() {"B00111100", "B00000100", "B00011100", "B00100000", "B00100000", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['6'] = new CharDefinition('6', new List<string>() {"B00011000", "B00000100", "B00000100", "B00011100", "B00100100", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['7'] = new CharDefinition('7', new List<string>() {"B00111100", "B00100000", "B00100000", "B00010000", "B00001000", "B00000100", "B00000100", "B00000000" } );
                    __CHAR_DIC['8'] = new CharDefinition('8', new List<string>() {"B00011000", "B00100100", "B00100100", "B00011000", "B00100100", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['9'] = new CharDefinition('9', new List<string>() {"B00011000", "B00100100", "B00100100", "B00111000", "B00100000", "B00100000", "B00011000", "B00000000" } );
                    __CHAR_DIC[':'] = new CharDefinition(':', new List<string>() {"B00000000", "B00000000", "B00000000", "B00000000", "B00000100", "B00000000", "B00000100", "B00000000" } );
                    __CHAR_DIC[';'] = new CharDefinition(';', new List<string>() {"B00000000", "B00000000", "B00000000", "B00000000", "B00001000", "B00000000", "B00001000", "B00000100" } );
                    __CHAR_DIC['<'] = new CharDefinition('<', new List<string>() {"B00000000", "B00000000", "B00010000", "B00001000", "B00000100", "B00001000", "B00010000", "B00000000" } );
                    __CHAR_DIC['='] = new CharDefinition('=', new List<string>() {"B00000000", "B00000000", "B00011100", "B00000000", "B00011100", "B00000000", "B00000000", "B00000000" } );
                    __CHAR_DIC['>'] = new CharDefinition('>', new List<string>() {"B00000000", "B00000000", "B00000100", "B00001000", "B00010000", "B00001000", "B00000100", "B00000000" } );
                    __CHAR_DIC['?'] = new CharDefinition('?', new List<string>() {"B00011000", "B00100100", "B00100000", "B00011000", "B00001000", "B00000000", "B00001000", "B00000000" } );
                    __CHAR_DIC['@'] = new CharDefinition('@', new List<string>() {"B00111000", "B01000100", "B01110100", "B01101100", "B00110100", "B00000100", "B00111000", "B00000000" } );
                    __CHAR_DIC['A'] = new CharDefinition('A', new List<string>() {"B00011000", "B00100100", "B00100100", "B00100100", "B00111100", "B00100100", "B00100100", "B00000000" } );
                    __CHAR_DIC['B'] = new CharDefinition('B', new List<string>() {"B00011100", "B00100100", "B00100100", "B00011100", "B00100100", "B00100100", "B00011100", "B00000000" } );
                    __CHAR_DIC['C'] = new CharDefinition('C', new List<string>() {"B00011000", "B00100100", "B00000100", "B00000100", "B00000100", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['D'] = new CharDefinition('D', new List<string>() {"B00011100", "B00100100", "B00100100", "B00100100", "B00100100", "B00100100", "B00011100", "B00000000" } );
                    __CHAR_DIC['E'] = new CharDefinition('E', new List<string>() {"B00111100", "B00000100", "B00000100", "B00011100", "B00000100", "B00000100", "B00111100", "B00000000" } );
                    __CHAR_DIC['F'] = new CharDefinition('F', new List<string>() {"B00111100", "B00000100", "B00000100", "B00011100", "B00000100", "B00000100", "B00000100", "B00000000" } );
                    __CHAR_DIC['G'] = new CharDefinition('G', new List<string>() {"B00011000", "B00100100", "B00000100", "B00110100", "B00100100", "B00100100", "B00111000", "B00000000" } );
                    __CHAR_DIC['H'] = new CharDefinition('H', new List<string>() {"B00100100", "B00100100", "B00100100", "B00111100", "B00100100", "B00100100", "B00100100", "B00000000" } );
                    __CHAR_DIC['I'] = new CharDefinition('I', new List<string>() {"B00011100", "B00001000", "B00001000", "B00001000", "B00001000", "B00001000", "B00011100", "B00000000" } );
                    __CHAR_DIC['J'] = new CharDefinition('J', new List<string>() {"B00110000", "B00100000", "B00100000", "B00100000", "B00100100", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['K'] = new CharDefinition('K', new List<string>() {"B00100100", "B00100100", "B00010100", "B00001100", "B00010100", "B00100100", "B00100100", "B00000000" } );
                    __CHAR_DIC['L'] = new CharDefinition('L', new List<string>() {"B00000100", "B00000100", "B00000100", "B00000100", "B00000100", "B00000100", "B00111100", "B00000000" } );
                    __CHAR_DIC['M'] = new CharDefinition('M', new List<string>() {"B01000100", "B01101100", "B01010100", "B01010100", "B01000100", "B01000100", "B01000100", "B00000000" } );
                    __CHAR_DIC['N'] = new CharDefinition('N', new List<string>() {"B01000100", "B01000100", "B01001100", "B01010100", "B01100100", "B01000100", "B01000100", "B00000000" } );
                    __CHAR_DIC['O'] = new CharDefinition('O', new List<string>() {"B00011000", "B00100100", "B00100100", "B00100100", "B00100100", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['P'] = new CharDefinition('P', new List<string>() {"B00011100", "B00100100", "B00100100", "B00011100", "B00000100", "B00000100", "B00000100", "B00000000" } );
                    __CHAR_DIC['Q'] = new CharDefinition('Q', new List<string>() {"B00011000", "B00100100", "B00100100", "B00100100", "B00100100", "B00100100", "B00011000", "B00100000" } );
                    __CHAR_DIC['R'] = new CharDefinition('R', new List<string>() {"B00011100", "B00100100", "B00100100", "B00011100", "B00100100", "B00100100", "B00100100", "B00000000" } );
                    __CHAR_DIC['S'] = new CharDefinition('S', new List<string>() {"B00011000", "B00100100", "B00000100", "B00011000", "B00100000", "B00100000", "B00011100", "B00000000" } );
                    __CHAR_DIC['T'] = new CharDefinition('T', new List<string>() {"B01111100", "B00010000", "B00010000", "B00010000", "B00010000", "B00010000", "B00010000", "B00000000" } );
                    __CHAR_DIC['U'] = new CharDefinition('U', new List<string>() {"B00100100", "B00100100", "B00100100", "B00100100", "B00100100", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['V'] = new CharDefinition('V', new List<string>() {"B01000100", "B01000100", "B01000100", "B01000100", "B00101000", "B00101000", "B00010000", "B00000000" } );
                    __CHAR_DIC['W'] = new CharDefinition('W', new List<string>() {"B01000100", "B01000100", "B01000100", "B01010100", "B01010100", "B01010100", "B00101000", "B00000000" } );
                    __CHAR_DIC['X'] = new CharDefinition('X', new List<string>() {"B01000100", "B01000100", "B00101000", "B00010000", "B00101000", "B01000100", "B01000100", "B00000000" } );
                    __CHAR_DIC['Y'] = new CharDefinition('Y', new List<string>() {"B01000100", "B01000100", "B01000100", "B00101000", "B00010000", "B00010000", "B00010000", "B00000000" } );
                    __CHAR_DIC['Z'] = new CharDefinition('Z', new List<string>() {"B00111100", "B00100000", "B00100000", "B00010000", "B00001000", "B00000100", "B00111100", "B00000000" } );
                    __CHAR_DIC['['] = new CharDefinition('[', new List<string>() {"B00001100", "B00000100", "B00000100", "B00000100", "B00000100", "B00000100", "B00001100", "B00000000" } );
                    __CHAR_DIC['\\']= new CharDefinition('\\',new List<string>() {"B00000100", "B00001000", "B00001000", "B00010000", "B00010000", "B00100000", "B00100000", "B00000000" } );
                    __CHAR_DIC[']'] = new CharDefinition(']', new List<string>() {"B00001100", "B00001000", "B00001000", "B00001000", "B00001000", "B00001000", "B00001100", "B00000000" } );
                    __CHAR_DIC['^'] = new CharDefinition('^', new List<string>() {"B00001000", "B00010100", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000" } );
                    __CHAR_DIC['_'] = new CharDefinition('_', new List<string>() {"B00000000", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000", "B00111100", "B00000000" } );
                    __CHAR_DIC['`'] = new CharDefinition('`', new List<string>() {"B00000100", "B00001000", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000", "B00000000" } );
                    __CHAR_DIC['a'] = new CharDefinition('a', new List<string>() {"B00000000", "B00000000", "B00011000", "B00100000", "B00111000", "B00100100", "B00111000", "B00000000" } );
                    __CHAR_DIC['b'] = new CharDefinition('b', new List<string>() {"B00000100", "B00000100", "B00011100", "B00100100", "B00100100", "B00100100", "B00011100", "B00000000" } );
                    __CHAR_DIC['c'] = new CharDefinition('c', new List<string>() {"B00000000", "B00000000", "B00011000", "B00100100", "B00000100", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['d'] = new CharDefinition('d', new List<string>() {"B00100000", "B00100000", "B00111000", "B00100100", "B00100100", "B00100100", "B00111000", "B00000000" } );
                    __CHAR_DIC['e'] = new CharDefinition('e', new List<string>() {"B00000000", "B00000000", "B00011000", "B00100100", "B00111100", "B00000100", "B00011000", "B00000000" } );
                    __CHAR_DIC['f'] = new CharDefinition('f', new List<string>() {"B00010000", "B00001000", "B00011100", "B00001000", "B00001000", "B00001000", "B00001000", "B00000000" } );
                    __CHAR_DIC['g'] = new CharDefinition('g', new List<string>() {"B00000000", "B00000000", "B00011000", "B00100100", "B00100100", "B00111000", "B00100000", "B00011100" } );
                    __CHAR_DIC['h'] = new CharDefinition('h', new List<string>() {"B00000100", "B00000100", "B00011100", "B00100100", "B00100100", "B00100100", "B00100100", "B00000000" } );
                    __CHAR_DIC['i'] = new CharDefinition('i', new List<string>() {"B00001000", "B00000000", "B00001100", "B00001000", "B00001000", "B00001000", "B00011100", "B00000000" } );
                    __CHAR_DIC['j'] = new CharDefinition('j', new List<string>() {"B00100000", "B00000000", "B00110000", "B00100000", "B00100000", "B00100000", "B00100100", "B00011000" } );
                    __CHAR_DIC['k'] = new CharDefinition('k', new List<string>() {"B00000100", "B00000100", "B00100100", "B00010100", "B00001100", "B00010100", "B00100100", "B00000000" } );
                    __CHAR_DIC['l'] = new CharDefinition('l', new List<string>() {"B00001100", "B00001000", "B00001000", "B00001000", "B00001000", "B00001000", "B00011100", "B00000000" } );
                    __CHAR_DIC['m'] = new CharDefinition('m', new List<string>() {"B00000000", "B00000000", "B00111100", "B01010100", "B01010100", "B01010100", "B01010100", "B00000000" } );
                    __CHAR_DIC['n'] = new CharDefinition('n', new List<string>() {"B00000000", "B00000000", "B00011100", "B00100100", "B00100100", "B00100100", "B00100100", "B00000000" } );
                    __CHAR_DIC['o'] = new CharDefinition('o', new List<string>() {"B00000000", "B00000000", "B00011000", "B00100100", "B00100100", "B00100100", "B00011000", "B00000000" } );
                    __CHAR_DIC['p'] = new CharDefinition('p', new List<string>() {"B00000000", "B00000000", "B00011100", "B00100100", "B00100100", "B00011100", "B00000100", "B00000100" } );
                    __CHAR_DIC['q'] = new CharDefinition('q', new List<string>() {"B00000000", "B00000000", "B00111000", "B00100100", "B00100100", "B00111000", "B00100000", "B00100000" } );
                    __CHAR_DIC['r'] = new CharDefinition('r', new List<string>() {"B00000000", "B00000000", "B00110100", "B00001100", "B00000100", "B00000100", "B00000100", "B00000000" } );
                    __CHAR_DIC['s'] = new CharDefinition('s', new List<string>() {"B00000000", "B00000000", "B00111000", "B00000100", "B00011000", "B00100000", "B00011100", "B00000000" } );
                    __CHAR_DIC['t'] = new CharDefinition('t', new List<string>() {"B00001000", "B00001000", "B00011100", "B00001000", "B00001000", "B00001000", "B00010000", "B00000000" } );
                    __CHAR_DIC['u'] = new CharDefinition('u', new List<string>() {"B00000000", "B00000000", "B00100100", "B00100100", "B00100100", "B00100100", "B00111000", "B00000000" } );
                    __CHAR_DIC['v'] = new CharDefinition('v', new List<string>() {"B00000000", "B00000000", "B01000100", "B01000100", "B01000100", "B00101000", "B00010000", "B00000000" } );
                    __CHAR_DIC['w'] = new CharDefinition('w', new List<string>() {"B00000000", "B00000000", "B01010100", "B01010100", "B01010100", "B01010100", "B00101000", "B00000000" } );
                    __CHAR_DIC['x'] = new CharDefinition('x', new List<string>() {"B00000000", "B00000000", "B01000100", "B00101000", "B00010000", "B00101000", "B01000100", "B00000000" } );
                    __CHAR_DIC['y'] = new CharDefinition('y', new List<string>() {"B00000000", "B00000000", "B00100100", "B00100100", "B00100100", "B00111000", "B00100000", "B00011100" } );
                    __CHAR_DIC['z'] = new CharDefinition('z', new List<string>() {"B00000000", "B00000000", "B00011100", "B00010000", "B00001000", "B00000100", "B00011100", "B00000000" } );
                    __CHAR_DIC['{'] = new CharDefinition('{', new List<string>() {"B00010000", "B00001000", "B00001000", "B00000100", "B00001000", "B00001000", "B00010000", "B00000000" } );
                    __CHAR_DIC['|'] = new CharDefinition('|', new List<string>() {"B00000100", "B00000100", "B00000100", "B00000100", "B00000100", "B00000100", "B00000100", "B00000000" } );
                    __CHAR_DIC['}'] = new CharDefinition('}', new List<string>() {"B00000100", "B00001000", "B00001000", "B00010000", "B00001000", "B00001000", "B00000100", "B00000000" } );
                    __CHAR_DIC['~'] = new CharDefinition('~', new List<string>() {"B00000000", "B00000000", "B00101000", "B00010100", "B00000000", "B00000000", "B00000000", "B00000000" } );
                }
                return __CHAR_DIC;
            }
        }

        
        /// <summary>
        /// Set all 8 Led's in a column to a new state
        /// Params:
        /// devIndex	address of the Display
        /// col	column which is to be set (0..7)
        /// value	each bit set to 1 will light up the
        /// 	corresponding Led.
        /// </summary>
        /// <param name="deviceIndex"></param>
        /// <param name="col"></param>
        /// <param name="value"></param>
        public void SetColumn(int deviceIndex, int col, byte value)
        {
            if(deviceIndex < 0 || deviceIndex >= this.MatrixCount)
                return;
            if(col < 0 || col > MATRIX_COL_SIZE-1) 
                return;

            //for(int row = 0; row < MATRIX_ROW_SIZE; row++) 
            for(int row = MATRIX_ROW_SIZE-1; row >=0; row--) 
            {
                //byte val = (byte)(value >> (MATRIX_ROW_SIZE - 1 - row));
                byte val = (byte)(value >> (row));
                val      = (byte)(val & 1);
                this.SetLed(deviceIndex, row, col, val != 0);
            }            
        }

        public List<bool> GetColumn(int deviceIndex, int colIndex)
        {
            var l = new List<bool>();
            int offset = deviceIndex * MATRIX_ROW_SIZE;
            var colIndexPower = (byte)Math.Pow(2, colIndex);
            for (int row = 0; row < MATRIX_ROW_SIZE; row++)
            {
                l.Add(BitUtil.IsSet((int)_pixels[offset + row], colIndexPower));
            }
            return l;
        }

        public List<bool> GetRow(int deviceIndex, int rowIndex)
        {
            var l = new List<bool>();
            int offset = deviceIndex * MATRIX_ROW_SIZE + rowIndex;
            for (int c = 0; c < MATRIX_COL_SIZE; c++)
            {
                var colIndexPower = (byte)Math.Pow(2, c);
                l.Add(BitUtil.IsSet((int)_pixels[offset], colIndexPower));
            }
            return l;
        }

        public void RotateRight(int deviceIndex)
        {
            this.RotateLeft(deviceIndex);
            this.RotateLeft(deviceIndex);
            this.RotateLeft(deviceIndex);
        }

        public void RotateLeft(int deviceIndex)
        {
            int offset = deviceIndex * MATRIX_ROW_SIZE;

            var valuesRow = new List<List<bool>>();
            for (var row = 0; row < MATRIX_ROW_SIZE; row++)
            {
                valuesRow.Add(GetRow(deviceIndex, row));
            }

            this.Clear(refresh: false, maxtrixIndex:deviceIndex);

            for (var col = 0; col < MATRIX_COL_SIZE; col++)
            {
                for (var row = 0; row < MATRIX_ROW_SIZE; row++)
                {
                    if (valuesRow[col][row])
                        _pixels[offset + row] = BitUtil.SetBit(_pixels[offset + row], (byte)Math.Pow(2, MATRIX_COL_SIZE - col - 1));
                }
            }
        }

        public void WriteChar(int deviceIndex, char character, bool clear = true)
        {
            if (!CHAR_DIC.ContainsKey(character))
                throw new ArgumentException(string.Format("Character '{0}' is not defined in CharDictionary"));

            if(clear)
                this.Clear(maxtrixIndex: deviceIndex);

            var charDef = __CHAR_DIC[character];

            var rr = MATRIX_ROW_SIZE - 1;
            for (var r = 0; r <MATRIX_ROW_SIZE; r++)
            {
                this._pixels[(MATRIX_ROW_SIZE*deviceIndex) + rr] = charDef.Bitmap[r];
                rr--;
            }
        }

        public NusbioMatrix WriteCharColumn(int deviceIndex, char character, int bitColumn, int targetColumn)
        {
            if (!CHAR_DIC.ContainsKey(character))
                throw new ArgumentException(string.Format("Character '{0}' is not defined in CharDictionary"));

            var charDef = __CHAR_DIC[character];
            var bits = new List<bool>();

            for (var r = 0; r < MATRIX_ROW_SIZE; r++)
            {
                var colIndexPower = (byte)Math.Pow(2, bitColumn);
                bits.Add(BitUtil.IsSet(charDef.Bitmap[r], colIndexPower));
            }

            var rr = MATRIX_ROW_SIZE - 1;
            for (var r = 0; r < MATRIX_ROW_SIZE; r++)
            {
                this.SetLed(deviceIndex, targetColumn, r, bits[rr]);
                rr--;
            }
            return this;
        }
    }
}
