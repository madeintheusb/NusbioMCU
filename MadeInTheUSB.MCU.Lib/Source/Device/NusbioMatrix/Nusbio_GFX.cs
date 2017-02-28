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
using MadeInTheUSB.Adafruit;

namespace MadeInTheUSB.MCU
{
    /// <summary>
    /// The Adafruit_GFX architecture requires inheritance, but NusbioMatrix must 
    /// inherits from NusbioMCU, so I used this class to be able to use Adafruit_GFX
    /// by NusbioMatrix without direct inheritance.
    /// </summary>
    public class Nusbio_GFX : Adafruit_GFX
    {
        private NusbioMatrix _nusbioMatrix;
        public Nusbio_GFX(int width, int height, NusbioMatrix nusbioMatrix) : base((short) width, (short) height)
        {
            _nusbioMatrix = nusbioMatrix;
        }
        public override void DrawPixel(short x, short y, ushort color)
        {
            _nusbioMatrix.SetLed(x, y, color == 1);
        }
    }
}