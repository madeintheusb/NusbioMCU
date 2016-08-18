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
    public partial class NusbioPixel : NusbioMCU, IDisposable
    {
        public int Count;

        public const int MAX_BRIGHTNESS_USB_POWER       = 64;
        public const int MAX_BRIGHTNESS_EXTERNAL_POWER  = 250;

        public int DEFAULT_BRIGHTNESS
        {
            get
            {
                if (this.Count <= 16)
                    return 16;
                if (this.Count < 60)
                    return 64;                
                return 64;
            }
        }

        public NusbioPixel(int baud = BAUD) : base(null, baud)
        {
            this._baud = baud;
        }

        public NusbioPixel(int ledCount, string comPort, int baud = BAUD) : base(comPort, baud)
        {
            this._comPort = comPort;
            this._baud = baud;
            this.Count = ledCount;
            this.ResetBytePerSecondCounters();
        }

        public override McuComResponse Initialize(Mcu.FirmwareName firmwareName = Mcu.FirmwareName.NusbioMcuMatrixPixel)
        {
            var r = base.Initialize(firmwareName);
            if (r.Succeeded)
            {
                if(this.SetBrightness(DEFAULT_BRIGHTNESS).Succeeded)
                    if (this.SetLedCount(this.Count).Succeeded)
                        return r;
            }
            return r;
        }

        private NusbioPixel CheckBreak(McuComResponse r)
        {
            if ((!r.Succeeded) && Debugger.IsAttached)
                Debugger.Break();
            return this;
        }

        public NusbioPixel Wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
            return this;
        }

        public NusbioPixel SetStrip(System.Drawing.Color color, int brigthness = -1)
        {
            if (brigthness != -1)
                this.SetBrightness(brigthness);

            this.SetPixel(0, color); // Set LED index to 0
            for (var i = 0; i < this.Count-1; i++)
                this.SetPixel(color); // Use next index
            var r = this.Show();
            if (r.Succeeded)
            {
                var ledDisplayedCount = r.GetParam(0);
            }
            return this;
        }

        public void Dispose()
        {
            this.SetStrip(Color.Red, this.DEFAULT_BRIGHTNESS);
            base.Close();
        }

        public McuComResponse SetLedCount(int count)
        {
            this.Count = count;
            return McuComResponse.Success;
            Send(Mcu.McuCommand.CP_RGB_PIXEL_SET_COUNT, (byte)count);
            var r = ReadAnswer();
            if (r.Succeeded)
            {
                if (r.GetParam(0) == count)
                    return r;
                else
                {
                    throw new ArgumentException();
                }
            }
            else return r;
        }

        public McuComResponse Show()
        {
            Send(Mcu.McuCommand.CP_RGB_PIXEL_DRAW, 0);
            var r = ReadAnswer();
            if (r.Succeeded)
            {
                var ledDisplayedCount = r.GetParam(0);
                return r;
            }
            else return r;
        }

        public McuComResponse SetPixel(int index, System.Drawing.Color color)
        {
            return this.SetPixel(index, color.R, color.G, color.B);
        }

        public McuComResponse SetPixel(System.Drawing.Color color)
        {
            return this.SetPixel(color.R, color.G, color.B);
        }

        public McuComResponse SetPixel(int r, int g, int b)
        {
            var buffer = new List<byte>();
            buffer.Add((byte)g);
            buffer.Add((byte)b);
            this.Send(Mcu.McuCommand.CP_RGB_PIXEL_SET_COLOR_NO_INDEX, r, buffer.ToArray());
            return McuComResponse.Success;
        }

        public McuComResponse SetPixel(int index, int r, int g, int b)
        {
            var buffer = new List<byte>();
            buffer.Add((byte)r);
            buffer.Add((byte)g);
            buffer.Add((byte)b);
            this.Send(Mcu.McuCommand.CP_RGB_PIXEL_SET_COLOR_1BYTE_INDEX, index, buffer.ToArray());
            return McuComResponse.Success;
        }

        
        public int GetMaxBrightness()
        {
            switch (base.PowerMode)
            {
                case PowerMode.USB: return MAX_BRIGHTNESS_USB_POWER;
                case PowerMode.EXTERNAL: return MAX_BRIGHTNESS_EXTERNAL_POWER;
                default: return DEFAULT_BRIGHTNESS;
            }
        }

        public McuComResponse SetBrightness(int brightness)
        {
            if (brightness > this.GetMaxBrightness())
                brightness = this.GetMaxBrightness();

            this.Send(Mcu.McuCommand.CP_RGB_PIXEL_SET_BRIGTHNESS, brightness);
            var r  = ReadAnswer();
            if (r.Succeeded)
            {
                if(r.GetParam(0) == brightness)
                    return r;
            }
            return r;
        }
    }
}
