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

namespace MadeInTheUSB.MCU
{
    public class NusbioLandscapeMatrix
    {
        public NusbioMatrix _nusbioMatrix;
        public int CurrentYPosition = 0;
        public int CurrentXPosition = 0;
        
        public NusbioLandscapeMatrix(NusbioMatrix nusbioMatrix, int deviceIndex)
        {
            this._nusbioMatrix     = nusbioMatrix;
            this.CurrentXPosition = this._nusbioMatrix.Width - 1;
            this.CurrentYPosition = this._nusbioMatrix.Height - 1;
            _nusbioMatrix.Clear(true);
        }

        private Random _seed = new Random(Environment.TickCount);

        private int NewDirectionRandomizer()
        {
            var r = _seed.Next(2);
            return r == 0 ? 1 : -1;
        }

        public override string ToString()
        {
            return string.Format("x:{0}, y:{1}", this.CurrentXPosition, this.CurrentYPosition);
        }

        public void Redraw()
        {
            _nusbioMatrix.ScrollPixelLeftDevices(1, 1);
            _nusbioMatrix.SetLed(CurrentXPosition, CurrentYPosition, true);
            _nusbioMatrix.WriteDisplay();
            
            CurrentYPosition += NewDirectionRandomizer();
            if (CurrentYPosition >= _nusbioMatrix.Height - 1)
                CurrentYPosition = _nusbioMatrix.Height - 1;
            if (CurrentYPosition < 0)
                CurrentYPosition = 0;
        }
    }
}