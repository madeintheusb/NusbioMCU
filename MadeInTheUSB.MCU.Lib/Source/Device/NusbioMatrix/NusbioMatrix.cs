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
    /// <summary>
    /// Control 1, 2, 4, or 8 8x8 LED matrix driven by 1, 2, 4 or 8 MAX7219
    /// chained.
    /// https://datasheets.maximintegrated.com/en/ds/MAX7219-MAX7221.pdf
    /// </summary>
    public partial class NusbioMatrix : NusbioMCU, IDisposable
    {
        /// <summary>
        /// The MAX7219 LED driver allows to chain up to 8 of itself
        /// </summary>
        public const int MAX7219_MAX_DEVICE = 8;
        /// <summary>
        /// The data for 1 8x8 matrix is 8 bytes. The MAX7219 LED driver 
        /// allows to chain up to 8 of itself. Therefore the max size buffer
        /// is 8x8 = 64
        /// </summary>
        public const int MAX7219_MAX_DATA_BUFFER = MAX7219_MAX_DEVICE * 8;
        /// <summary>
        /// Limit to 3 rather than 16, to avoid going above 200 mA when using 
        /// a 4 8x8 matrix. The ATMega328 blue breakout will reboot if consumption goes
        /// above 200 mA. The RobotDyn ATMega328 black breakout can handle up 450 mA
        /// but that is too much.
        /// Every 1 level of intensisty consume about 50 mA with a 4 8x8 matrix
        /// if all 256 LEDs are on.
        /// </summary>
        public const int MAX7219_MAX_INTENSITY = 4; // from 0..4
        /// <summary>
        /// Minimun LED intensity allowed
        /// </summary>
        public const int MAX7219_MIN_INTENSITY = 0;

        public const int MATRIX_ROW_SIZE = 8;
        public const int MATRIX_COL_SIZE = 8;

        /// <summary>
        /// Internal data representation
        /// </summary>
        private byte[] _pixels = new byte[MAX7219_MAX_DATA_BUFFER];

        /// <summary>
        /// The number of matrix currently initialized
        /// </summary>
        public int MatrixCount;

        public int Intensity;

        public int LedCount;

        public NusbioMatrix(int baud = BAUD) : base(null, baud)
        {
            _nusbio_gfx = new Nusbio_GFX(4*8, 8, this);
        }

        public NusbioMatrix(string comPort, int maxtrixCount, int baud = BAUD) : base(comPort, baud)
        {
            this._nusbio_gfx    = new Nusbio_GFX(maxtrixCount * 8, 8, this);
            this.MatrixCount    = maxtrixCount;
        }

        public override McuComResponse Initialize(List<Mcu.FirmwareName> firmwareNames = null)
        {
            var r = base.Initialize(firmwareNames);
            if (r.Succeeded)
            {
                if (this.SetMatrixCount(this.MatrixCount).Succeeded)
                    if (this.SetIntensity(0).Succeeded)
                        return base.InitializationDone();
            }
            return r;
        }

        public NusbioMatrix SetLed(int x, int y, bool on)
        {
            int matrixIndex = 0;

            if (this.MatrixCount > 1)
            {
                // Because of the way the 4 MAX7219/Matrix are wired, to provide
                // an quater of an axis, we need to transform the x this way
                var x8 = (x / 8);
                //matrixIndex = this.MatrixCount - x8 - 1;
                matrixIndex = x8;
                x = x - (x8 * MATRIX_COL_SIZE);
            }
            // Because of the way the 4 MAX7219/Matrix are wired, to provide
            // an quater of an axis, we need to transform the x this way
            //y = MATRIX_ROW_SIZE - y - 1; // Reverse y internally
            this.SetLed(matrixIndex, x, y, on);
            return this;
        }

        private void SetLed(int deviceIndex, int column, int row, Boolean state, bool refresh = false)
        {
            byte val = 0x00;

            if (deviceIndex < 0 || deviceIndex >= this.MatrixCount)
                return;

            if (row < 0 || row > MATRIX_COL_SIZE - 1 || column < 0 || column > MATRIX_COL_SIZE - 1)
                return;

            int offset = deviceIndex * MATRIX_ROW_SIZE;

            //val = (byte)(128 >> column);
            val = (byte)(1 << column);
            if (state)
                _pixels[offset + row] = (byte)(_pixels[offset + row] | val);
            else
            {
                val = (byte)(~val);
                _pixels[offset + row] = (byte)(_pixels[offset + row] & val);
            }
            if (refresh)
                this.WriteDisplay();
        }

        //public string GetenerateCode()
        //{
        //    var rr = new StringBuilder();
        //    var buffer = this._pixels.Take(this.MatrixCount*MATRIX_ROW_SIZE).ToList();

        //    var b = new List<byte>();
        //    for (var r = 0; r < NusbioMatrix.MATRIX_ROW_SIZE; r++)
        //    {
        //        for (var devIndex = 0; devIndex < this.MatrixCount; devIndex++)
        //        {
        //            b.Add(this._pixels[(devIndex * NusbioMatrix.MATRIX_ROW_SIZE) + r]);
        //        }
        //    }

        //    foreach (var by in b)
        //    {
        //        rr.AppendFormat("{0}, ", by);
        //    }
        //    return rr.ToString();
        //}

        private McuComResponse __writeDisplay()
        {
            var r = this.DrawFrame();
            if (!r.Succeeded)
            {
                if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
            }
            return r;
        }

        public NusbioMatrix Clear(bool refresh = false, int maxtrixIndex = -1)
        {
            if (maxtrixIndex == -1)
            {
                for (var i = 0; i < this._pixels.Length; i++) // Reset all matrices
                    this._pixels[i] = 0;
            }
            else
            {
                for (var r = 0; r < 8; r++)
                    for (var c = 0; c < 8; c++)
                        this.SetLed(maxtrixIndex, c, r, false);
            }
            if (refresh)
                this.__writeDisplay();
            return this;
        }

        public McuComResponse DrawFrame(int value)
        {
            var buffer = new List<byte>();
            for (var i = 0; i < this.MatrixCount * MATRIX_ROW_SIZE; i++)
                buffer.Add((byte)value);

            this.Send(Mcu.McuCommand.CP_DRAWFRAME, buffer.ToArray());
            return McuComResponse.Success;
        }

        private McuComResponse __writeRow(int row)
        {
            var b = new List<byte>();

            for (var devIndex = 0; devIndex < this.MatrixCount; devIndex++)
                b.Add(this._pixels[(devIndex * NusbioMatrix.MATRIX_ROW_SIZE) + row]);

            this.Send(Mcu.McuCommand.CP_DRAWROW, row, b.ToArray());
            return McuComResponse.Success;
        }

        private McuComResponse DrawFrame()
        {
            var b = new List<byte>();
            for (var r = 0; r < NusbioMatrix.MATRIX_ROW_SIZE; r++)
            {
                for (var devIndex = 0; devIndex < this.MatrixCount; devIndex++)
                    b.Add(this._pixels[(devIndex * NusbioMatrix.MATRIX_ROW_SIZE) + r]);
            }
            this.Send(Mcu.McuCommand.CP_DRAWFRAME, b.ToArray());
            return McuComResponse.Success;
        }

        public McuComResponse SetMatrixCount(int maxtrixCount)
        {
            this.MatrixCount = maxtrixCount;
            Send(Mcu.McuCommand.CP_SETDEVICECOUNT, (byte)maxtrixCount);
            var r = ReadAnswer();
            if (r.Succeeded)
            {
                if (r.GetParam(0) == MatrixCount)
                    return r;
                else
                    return r.Fail("Internal error on SetMatrixCount()");
            }
            else return r;
        }

        public McuComResponse SetIntensity(int intensity)
        {
            if (intensity > MAX7219_MAX_INTENSITY)
                intensity = MAX7219_MAX_INTENSITY;

            if (intensity < MAX7219_MIN_INTENSITY)
                intensity = MAX7219_MIN_INTENSITY;

            this.Intensity = intensity;

            Send(Mcu.McuCommand.CP_SETINTENSITY, (byte)intensity);
            return ReadAnswer();
        }

        public int GetMatrixCount()
        {
            Send(Mcu.McuCommand.CP_GETDEVICECOUNT);
            var r = ReadAnswer();
            if (r.Succeeded)
                return r.GetParam(0);
            else
            {
                return -1;
            }
        }
        
        public NusbioMatrix Rectangle(Rectangle r, bool on)
        {
            this.DrawRect(r, on);
            return this;
        }

        public NusbioMatrix Circle(int x0, int y0, int r, bool color)
        {
            if (r > 0)
                this.DrawCircle(x0, y0, r, color);
            return this;
        }

        public NusbioMatrix Sleep(int ms)
        {
            Thread.Sleep(ms);
            return this;
        }

        public NusbioMatrix WriteDisplay()
        {
            return CheckBreak(this.__writeDisplay());
        }

        public NusbioMatrix WriteRow(int r)
        {
            return CheckBreak(this.__writeRow(r));
        }

        private NusbioMatrix CheckBreak(McuComResponse r)
        {
            if ((!r.Succeeded) && Debugger.IsAttached)
                Debugger.Break();
            return this;
        }

        public override void Close()
        {
            base.Close();
        }

        public void Dispose()
        {
            base.Close();
        }

        //public McuComResponse NusbioPixel_SetLedCount(int count)
        //{
        //    this.LedCount = count;
        //    Send(Mcu.McuCommand.CP_RGB_PIXEL_SET_COUNT, (byte)count);
        //    return ReadAnswer();
        //}

        //public McuComResponse NusbioPixel_Show()
        //{
        //    Send(Mcu.McuCommand.CP_RGB_PIXEL_DRAW, 0);
        //    return ReadAnswer();
        //}

        //public McuComResponse NusbioPixel_SetPixel(int r, int g, int b)
        //{
        //    var buffer = new List<byte>();
        //    buffer.Add((byte)g);
        //    buffer.Add((byte)b);
        //    this.Send(Mcu.McuCommand.CP_RGB_PIXEL_SET_COLOR_NO_INDEX, r, buffer.ToArray());
        //    return McuComResponse.Success;
        //}

        //public McuComResponse NusbioPixel_SetPixel(int index, int r, int g, int b)
        //{
        //    var buffer = new List<byte>();
        //    buffer.Add((byte)r);
        //    buffer.Add((byte)g);
        //    buffer.Add((byte)b);
        //    this.Send(Mcu.McuCommand.CP_RGB_PIXEL_SET_COLOR_1BYTE_INDEX, index, buffer.ToArray());
        //    return McuComResponse.Success;
        //}

        //public McuComResponse NusbioPixel_SetBrightness(int brightness)
        //{
        //    var buffer = new List<byte>();
        //    this.Send(Mcu.McuCommand.CP_RGB_PIXEL_SET_BRIGTHNESS, brightness, buffer.ToArray());
        //    return McuComResponse.Success;
        //}
    }
}
