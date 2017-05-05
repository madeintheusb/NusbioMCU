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
using System.Diagnostics;

namespace MadeInTheUSB.MCU
{
    public partial class PerformanceTrackerImpl : IPerformanceTracker
    {
        private DateTime    _bytePerSecondStartTime;
        private TimeSpan    _bytePerSecondDuration;
        private long        _bytePerSecondByteCount;
        private long        _bytePerSecondByteTotalForSessionCount;

        public void AddByte(long byteCount)
        {
            this._bytePerSecondByteCount += byteCount;
            this._bytePerSecondByteTotalForSessionCount  += byteCount;
        }

        public void ResetBytePerSecondCounters()
        {
            this._bytePerSecondStartTime = DateTime.Now;
            this._bytePerSecondByteCount = 0;
        }

        public string GetByteSecondSentStatus(bool reset = false)
        {
            var s = string.Format(
                "{0:00.0} KByte/S, {1:0000} Bytes, {2:00.0} ms    ",
                this.GetKByteSecondSent(),
                this._bytePerSecondByteCount,
                this._bytePerSecondDuration.TotalMilliseconds
                );

            if (reset)
                this.ResetBytePerSecondCounters();

            return s;
        }

        private double GetKByteSecondSent()
        {
            try
            {
                this._bytePerSecondDuration = (DateTime.Now - this._bytePerSecondStartTime);
                double bytePerSecond = this._bytePerSecondByteCount/(this._bytePerSecondDuration.TotalMilliseconds/1000.0);
                return bytePerSecond/1024;
            }
            catch
            {
            }
            return -1;
        }
    }
}