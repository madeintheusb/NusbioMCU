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
//[MadeInTheUSB.MCU.StripIndex]::S0
namespace MadeInTheUSB.MCU
{
    public enum NusbioPixelDeviceType
    {
        Unknown = 0,
        Bar10 = 10,
        Strip30 = 30,
        Strip60 = 60,
        Ring12 = 12,
        Square16 = 16,
        Square8x8 = 64,
        Strip300 = 300,
        Strip180 = 180,
    }

    public partial class NusbioPixel : NusbioMCU, IDisposable
    {
        public int Count;

        public const int DEFAULT_PIXEL_COUNT = 60;

        public const int MAX_BRIGHTNESS_USB_POWER = 64;
        public const int MAX_BRIGHTNESS_USB_POWER_33_LED_OR_MORE = 48;

        public const int MAX_BRIGHTNESS_EXTERNAL_POWER = 250;

        public const int NUSBIO_MCU_MAX_LED = 64;
        public const int NUSBIO_PIXELS_MCU_MAX_LED = 180;

        public enum StripIndex
        {
            S0 = 1,
            S1 = 2
        }

        /// <summary>
        /// A 60 LED Strip requires the 500 mF capacitor
        /// a max brightness of 64
        /// </summary>
        public int DEFAULT_BRIGHTNESS
        {
            get
            {
                if (this.Count >= 30)
                    return 48;
                return 64;
            }
        }

        private static NusbioPixel SetNusbioPixelAsConnected(NusbioPixel nusbioPixel, Color c)
        {
            if (nusbioPixel.Firmware == Mcu.FirmwareName.NusbioMcu2StripPixels)
                nusbioPixel.SetStrip(c, nusbioPixel.DEFAULT_BRIGHTNESS, StripIndex.S1);

            nusbioPixel.SetStrip(c, nusbioPixel.DEFAULT_BRIGHTNESS, StripIndex.S0);

            return nusbioPixel;
        }

        public static NusbioPixel ConnectToMCU(NusbioPixel nusbioPixel, int maxLed)
        {
            if (nusbioPixel != null)
            {
                nusbioPixel.Dispose();
                nusbioPixel = null;
            }
            var comPort = new NusbioPixel().DetectMcuComPort();
            if (comPort == null)
            {
                Console.WriteLine("Nusbio Pixel not detected");
                return null;
            }
            nusbioPixel = new NusbioPixel(maxLed, comPort);
            if (nusbioPixel.Initialize().Succeeded)
            {
                if (nusbioPixel.SetBrightness(nusbioPixel.DEFAULT_BRIGHTNESS).Succeeded)
                {
                    if (nusbioPixel.Firmware == Mcu.FirmwareName.NusbioMcu2StripPixels)
                    {
                        nusbioPixel.PowerMode = PowerMode.EXTERNAL;
                        if (nusbioPixel.SetBrightness(nusbioPixel.DEFAULT_BRIGHTNESS, NusbioPixel.StripIndex.S1).Succeeded)
                            return SetNusbioPixelAsConnected(nusbioPixel, Color.Green);
                    }
                    else return SetNusbioPixelAsConnected(nusbioPixel, Color.Green);
                }
            }
            return null;
        }

        public NusbioPixel() : this(BAUD)
        {

        }

        public NusbioPixel(int baud) : base(null, baud)
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

        public override McuComResponse Initialize(List<Mcu.FirmwareName> firmwareNames = null)
        {
            if (firmwareNames == null)
                firmwareNames = new List<Mcu.FirmwareName>();

            if (firmwareNames.Count == 0)
            {
                firmwareNames.Add(Mcu.FirmwareName.NusbioMcuMatrixPixel);
                firmwareNames.Add(Mcu.FirmwareName.NusbioMcu2StripPixels);
            }
            var r = base.Initialize(firmwareNames);
            if (r.Succeeded)
            {
                if (this.SetBrightness(DEFAULT_BRIGHTNESS).Succeeded)
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

        public NusbioPixel SetStrip(System.Drawing.Color color)
        {
            return this.SetStrip(color, -1, StripIndex.S0);
        }

        public NusbioPixel SetStrip(System.Drawing.Color color, int brigthness)
        {
            return this.SetStrip(color, brigthness, StripIndex.S0);
        }

        public NusbioPixel SetStrip(System.Drawing.Color color, StripIndex stripIndex)
        {
            return this.SetStrip(color, -1, stripIndex);
        }

        public NusbioPixel SetStrip(System.Drawing.Color color, int brigthness, StripIndex stripIndex)
        {
            if (brigthness != -1)
                this.SetBrightness(brigthness, stripIndex: stripIndex);

            this.SetPixel(0, color, stripIndex: stripIndex); // Set LED index to 0
            for (var i = 0; i < this.Count - 1; i++)
                this.SetPixel(color, stripIndex: stripIndex); // Use next index

            var r = this.Show(stripIndex: stripIndex);

            if (!r.Succeeded && System.Diagnostics.Debugger.IsAttached)
                Debugger.Break();

            return this;
        }

        public void Dispose()
        {
            SetNusbioPixelAsConnected(this, Color.Red);
            base.Close();
        }

        public McuComResponse SetLedCount(int count, StripIndex stripIndex = StripIndex.S0)
        {
            this.Count = count;

            if (this.Firmware == Mcu.FirmwareName.NusbioMcuMatrixPixel && count > NUSBIO_MCU_MAX_LED)
                count = NUSBIO_MCU_MAX_LED;
            if (this.Firmware == Mcu.FirmwareName.NusbioMcu2StripPixels && count > NUSBIO_PIXELS_MCU_MAX_LED)
                count = NUSBIO_PIXELS_MCU_MAX_LED;

            var rr = new McuComResponse();
            return McuComResponse.Success;
            /*
            Send(HandleStripIndex(Mcu.McuCommand.CP_RGB_PIXEL_SET_COUNT, stripIndex), (byte)count);
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
            */
        }

        public McuComResponse Show(
            StripIndex stripIndex = StripIndex.S0,
            int minimumWait = 16 // By experimentation it takes between 15 to 30ms to execute the api call with a 60 LED strip
            )
        {
            Send(HandleStripIndex(Mcu.McuCommand.CP_RGB_PIXEL_DRAW, stripIndex), 0);
            /*
            var r = ReadAnswer();
            if (r.Succeeded)
            {
                if (r.GetParam(0) == (int)stripIndex)
                    return r;
                else
                    return r.Fail("Show() did not return expected value");
            }
            */
            // Show pixel we do not read the answer
            Thread.Sleep(minimumWait);
            return McuComResponse.Success;
        }

        public McuComResponse SetPixel(int index, System.Drawing.Color color, int count, bool refresh = false, StripIndex stripIndex = StripIndex.S0)
        {
            var r = new McuComResponse();
            for (var i = 0; i < count; i++)
            {
                r = this.SetPixel(i, color, stripIndex: stripIndex);
                if (!r.Succeeded) return r;
            }
            if (refresh)
                this.Show();
            return r;
        }

        /// <summary>
        /// Implemented different variant of SetPixel() with overload to be compatible
        //  with PowerShell. PowerShell seems to have an issue with optional parameters 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="color"></param>
        /// <param name="optimized"></param>
        /// <returns></returns>
        public McuComResponse SetPixel(int index, System.Drawing.Color color, bool optimized)
        {
            if (index == 0)
                return this.SetPixel(index, color);
            else
                return this.SetPixel(color);
        }

        public McuComResponse SetPixel(int index, Color color)
        {
            return SetPixel(index, color, StripIndex.S0);
        }

        public McuComResponse SetPixel(int index, Color color, StripIndex stripIndex)
        {
            return this.SetPixel(index, color.R, color.G, color.B, stripIndex: stripIndex);
        }

        public McuComResponse SetPixel(System.Drawing.Color color)
        {
            return this.SetPixel(color, StripIndex.S0);
        }

        public McuComResponse SetPixel(Color color, StripIndex stripIndex)
        {
            return this.SetPixel(color.R, color.G, color.B, stripIndex: stripIndex);
        }

        public McuComResponse SetPixel(int r, int g, int b)
        {
            return this.SetPixel(r, g, b, StripIndex.S0);
        }

        public McuComResponse SetPixel(int r, int g, int b, StripIndex stripIndex)
        {
            var buffer = new List<byte>();
            buffer.Add((byte)g);
            buffer.Add((byte)b);
            this.Send(HandleStripIndex(Mcu.McuCommand.CP_RGB_PIXEL_SET_COLOR_NO_INDEX, stripIndex), r, buffer.ToArray());

            return McuComResponse.Success;
        }

        public McuComResponse SetPixel(int index, int r, int g, int b)
        {
            return SetPixel(index, r, g, b, StripIndex.S0);
        }

        public McuComResponse SetPixel(int index, int r, int g, int b, StripIndex stripIndex)
        {
            var buffer = new List<byte>();
            buffer.Add((byte)r);
            buffer.Add((byte)g);
            buffer.Add((byte)b);
            this.Send(HandleStripIndex(Mcu.McuCommand.CP_RGB_PIXEL_SET_COLOR_1BYTE_INDEX, stripIndex), index, buffer.ToArray());

            return McuComResponse.Success;
        }

        private Mcu.McuCommand HandleStripIndex(Mcu.McuCommand c, StripIndex stripIndex)
        {
            var cc = c;
            if (stripIndex == StripIndex.S1)
                c = (Mcu.McuCommand)(((int)c) + Mcu.CP_RGB_PIXEL_2_STRIP_CMD_OFFSET);
            return c;
        }

        public int GetMaxBrightness()
        {
            switch (base.PowerMode)
            {
                case PowerMode.USB:
                    if (this.Count <= 32)
                        return MAX_BRIGHTNESS_USB_POWER;
                    else
                        return MAX_BRIGHTNESS_USB_POWER_33_LED_OR_MORE;

                case PowerMode.EXTERNAL: return MAX_BRIGHTNESS_EXTERNAL_POWER;
                default: return DEFAULT_BRIGHTNESS;
            }
        }

        public McuComResponse SetBrightness(int brightness, StripIndex stripIndex = StripIndex.S0)
        {
            if (brightness > this.GetMaxBrightness())
                brightness = this.GetMaxBrightness();

            this.Send(HandleStripIndex(Mcu.McuCommand.CP_RGB_PIXEL_SET_BRIGTHNESS, stripIndex), brightness);
            var r = ReadAnswer();
            if (r.Succeeded)
            {
                if (r.GetParam(0) == brightness - ((int)stripIndex))
                    return r;
            }
            return r;
        }
    }
}



