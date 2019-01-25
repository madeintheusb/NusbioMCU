/*
    NusbioMatrix/NusbioPixel devices for Windows/.NET
    MadeInTheUSB MCU ATMega328 Based Device
    Copyright (C) 2016,2017,2019 MadeInTheUSB LLC 

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
using MadeInTheUSB.Adafruit;
using System.Collections.Generic;
using System.Drawing;

namespace MadeInTheUSB.MCU
{
    /// <summary>
    /// The Adafruit_GFX architecture requires inheritance, but NusbioMatrix must 
    /// inherits from NusbioMCU, so I used this class to be able to use Adafruit_GFX
    /// by NusbioMatrix without direct inheritance.
    /// </summary>
    public class Matrix8x8 : Adafruit_GFX_DrawingColor
    {
        private NusbioPixel _nusbioPixel;

        public Matrix8x8(NusbioPixel nusbioPixel, int width = 8, int height = 8) : base((short) width, (short) height)
        {
            _nusbioPixel = nusbioPixel;
        }

        public override void DrawPixel(short x, short y, Color color)
        {
            _nusbioPixel.SetPixel(Matrix8x8GetPixelAddr(x, y), color);
        }

        public Matrix8x8 Show()
        {
            _nusbioPixel.Show();
            return this;
        }

        public Matrix8x8 Clear(Color color)
        {
            _nusbioPixel.SetStrip(color);
            return this;
        }

        public Matrix8x8 Wait(int ms)
        {
            _nusbioPixel.Wait(ms);
            return this;
        }

        /*
        * In a 8x8 matrix the pixel are aligned this way
        * row 0, col 0..7 -> 0, 1, 2, 3, 4, 5, 6, 7
        * row 1, col 0..7 -> 15, 14, 13, 12, 11, 10, 9, 8
        * row 2, col 0..7 -> 16, 17, 18, 19, 20, 21 , 22, 23
        */
        public static int Matrix8x8GetPixelAddr(int x, int y)
        {
            var ret = 0;
            var evenRows = new List<int>() { 0, 2, 4, 6 };
            var oddRows = new List<int>() { 1, 3, 5, 7 };
            if (evenRows.Contains(y))
            {
                ret = (y * 8) + x;
            }
            else if (oddRows.Contains(y))
            {
                ret = (y * 8) + (8 - x - 1);
            }
            return ret;
        }
    }
}