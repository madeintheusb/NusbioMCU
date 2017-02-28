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
using MadeInTheUSB.Communication;
using MadeInTheUSB.WinUtil;
using System.Diagnostics;

namespace MadeInTheUSB.MCU
{
    /// <summary>
    /// The Adafruit_GFX architecture requires inheritance, but NusbioMatrix must 
    /// inherits from NusbioMCU, so I used this class to be able to use Adafruit_GFX
    /// by NusbioMatrix without direct inheritance.
    /// </summary>
    public partial class NusbioMatrix : NusbioMCU, IDisposable
    {
        private Nusbio_GFX _nusbio_gfx;
        public int Width
        {
            get { return _nusbio_gfx.Width; }
        }
        public int Height
        {
            get { return _nusbio_gfx.Height; }
        }

        public void DrawBitmap(int x, int y, List<string> bitmapDefinedAsString, int w, int h, bool color)
        {
            _nusbio_gfx.DrawBitmap(x, y, bitmapDefinedAsString, w, h, color);
        }

        public void DrawCircle(int x0, int y0, int r, bool color)
        {
            _nusbio_gfx.DrawCircle(x0, y0, r, color);
        }

        public void DrawRect(Rectangle r, bool on)
        {
            _nusbio_gfx.DrawRect(r.X, r.Y, r.Width, r.Height, on);
        }

        public void DrawRect(int x, int y, int w, int h, bool on)
        {
            _nusbio_gfx.DrawRect(x, y, w, h, on);
        }

        public void DrawRoundRect(int x, int y, int w, int h, int r, int color)
        {
            _nusbio_gfx.DrawRoundRect(x, y, w, h, r, color);
        }

        public void DrawLine(int x0, int y0, int x1, int y1, bool color)
        {
            _nusbio_gfx.DrawLine(x0, y0, x1, y1, color);
        }

    }
}
