/*
    NusbioMatrix/NusbioPixel devices for Windows/.NET
    MadeInTheUSB MCU ATMega328 Based Device
    Copyright (C) 2016 MadeInTheUSB LLC 

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
    public partial class FutabaLCD : NusbioMCU, IDisposable
    {
        public int Count  = 1; // 1 line

        public FutabaLCD(int baud = BAUD) : base(null, baud)
        {
            this._baud = baud;
        }

        public FutabaLCD(string comPort, int baud = BAUD) : base(comPort, baud)
        {
            this._comPort = comPort;
            this._baud = baud;
            this.ResetBytePerSecondCounters();
        }

        public override McuComResponse Initialize(Mcu.FirmwareName firmwareName = Mcu.FirmwareName.NusbioMcuMatrixPixel)
        {
            var r = base.Initialize(firmwareName);
            if (r.Succeeded)
            {
                return r;
            }
            return r;
        }

        private FutabaLCD CheckBreak(McuComResponse r)
        {
            if ((!r.Succeeded) && Debugger.IsAttached)
                Debugger.Break();
            return this;
        }

        public FutabaLCD Wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
            return this;
        }

        public void Dispose()
        {
            base.Close();
        }

        public McuComResponse RequestTest()
        {
            return this.SetCursor(0, 0);
        }

        public McuComResponse SetCursor(int x, int y)
        {
            var buffer = new List<byte>();
            buffer.Add((byte)y);
            this.Send(Mcu.McuCommand.CP_FUTABA_LCD_SETCURSOR, x, buffer.ToArray());
            var r = ReadAnswer();
            if (r.Succeeded)
            {
                return r;
            }
            else
            {
                return r;
            }
        }        
    }
}
