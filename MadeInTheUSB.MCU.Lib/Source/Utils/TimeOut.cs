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

namespace MadeInTheUSB
{
    public class TimeOut
    {
        public int Counter;

        private int _duration;
        private int _time;

        public TimeOut(int duration)
        {
            this.Counter = 0;
            this._duration = duration;
            Reset();
        }
        
        public Boolean IsFirstTime()
        {
            return this.Counter == 1;
        }
        
        public Boolean IsTimeOut(bool isFirstTime = false)
        {
            if (isFirstTime && this.IsFirstTime())
            {
                this.Reset();
                return true;
            }

            Boolean b = (Environment.TickCount - this._time) > this._duration;
            if (b)
            {
                this.Reset();
            }
            return b;
        }

        private Boolean EveryCalls(int callCount)
        {
            return Counter % callCount == 0;
        }

        public void Reset()
        {
            this._time = Environment.TickCount;
            this.Counter++;
        }

        public override string ToString()
        {
            return String.Format("TimeOut counter:{0}, duration:{1}, time:{2}", this.Counter, this._duration, this._time);
        }
    }
}
