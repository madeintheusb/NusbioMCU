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
using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t = System.Byte;

namespace MadeInTheUSB.MCU
{
    public partial class NusbioMatrix :  IDisposable
    {
        public NusbioMatrix  ScrollPixelLeftDevices(int deviceIndex, int scrollCount = 1)
        {
            if (scrollCount > 1)
            {
                for (var i = 0; i < scrollCount; i++)
                    ScrollPixelLeftDevices(deviceIndex);
            }
            else
            {
                if (this.MatrixCount == 1)
                {
                    this.ScrollLeft(0);
                    return this;
                }

                var index = deviceIndex;
                bool scrollFirstMaxtrix = true;
                while (true)
                {
                    __ScrollLeftDevices(index, scrollFirstMaxtrix);
                    if (index == this.MatrixCount-1)
                        break;
                    index++;
                    scrollFirstMaxtrix = false;
                }
            }
            return this;
        }
        
        private void __ScrollLeftDevices(int deviceIndex, bool scrollFirstMaxtrix)
        {
            int offsetSrc = deviceIndex  * MATRIX_ROW_SIZE;

            if (scrollFirstMaxtrix)
            {
                for (var r = 0; r < MATRIX_COL_SIZE; r++)
                {
                    ScrollLeftColumn(deviceIndex-1, r);
                }
            }

            // Save the first column from the matrix on the right
            var tmpBit = new bool[MATRIX_COL_SIZE];
            for (var r = 0; r < MATRIX_COL_SIZE; r++)
            {
                tmpBit[r] = BitUtil.IsSet(_pixels[offsetSrc + r], 1);
                //tmpBit[r] = BitUtil.IsSet(_pixels[offsetSrc + r], 128);
            }

            // Scroll the second matrix on the right, we lose a column of pixel
            for (var r = 0; r < MATRIX_COL_SIZE; r++)
            {
                ScrollLeftColumn(deviceIndex, r);
            }

            // Copy the first column of second matrix on the right to matrix on the left
            for (var r = 0; r < MATRIX_COL_SIZE; r++)
            {
                this.SetLed(deviceIndex-1, MATRIX_COL_SIZE-1, r, tmpBit[r]);
            }
        }

        public NusbioMatrix  ScrollPixelRightDevices(int deviceIndexSrc, int deviceIndexDest, int scrollCount = 1)
        {
            if (scrollCount > 1)
            {
                for (var i = 0; i < scrollCount; i++)
                    ScrollPixelRightDevices(deviceIndexSrc, deviceIndexDest);
            }
            else
            {
                if (this.MatrixCount == 1)
                {
                    this.ScrollDown(0);
                    return this;
                }

                var index = deviceIndexSrc;
                bool scrollFirstMaxtrix = true;
                while (true)
                {
                    __ScrollRightDevices(index, index - 1, scrollFirstMaxtrix);
                    if (index - 1 == deviceIndexDest)
                        break;
                    index--;
                    scrollFirstMaxtrix = false;
                }
            }
            return this;
        }

        private void __ScrollRightDevices(int deviceIndexSrc, int deviceIndexDest, bool scrollFirstMaxtrix)
        {
            int offsetSrc  = deviceIndexSrc  * MATRIX_ROW_SIZE;
            int offsetDest = deviceIndexDest * MATRIX_ROW_SIZE;

            if (scrollFirstMaxtrix)
            {
                // Scroll the first matrix on the left, we lose a column of pixel
                for (var r = 0; r < MATRIX_ROW_SIZE; r++)
                {
                    ScrollRightColumn(deviceIndexSrc, r);
                    this.WriteDisplay();
                }
            }

            // Save the first column from the matrix on the right
            var tmpBit = new bool[MATRIX_COL_SIZE];
            for (var r = 0; r < MATRIX_ROW_SIZE; r++)
            {
                //tmpBit[r] = BitUtil.IsSet(_pixels[offsetDest + r], 1);
                tmpBit[r] = BitUtil.IsSet(_pixels[offsetDest + r], 128);
            }

            // Scroll the second matrix on the right, we lose a column of pixel
            for (var r = 0; r < MATRIX_ROW_SIZE; r++)
            {
                ScrollRightColumn(deviceIndexDest, r);
                this.WriteDisplay();
            }

            // Copy the first column of second matrix on the right to matrix on the left
            for (var r = 0; r < MATRIX_ROW_SIZE; r++)
            {
                this.SetLed(deviceIndexSrc, 0, r, tmpBit[r]);
                this.WriteDisplay();
            }
        }

        public void ShiftRight(bool rotate, bool fillZero)
        {
            int last = this.MatrixCount * 8 - 1;
            byte old = this._pixels[last];
            for (var i = this._pixels.Length-1; i > 0; i--)
	            this._pixels[i] = this._pixels[i-1];
            if (rotate) 
                this._pixels[0] = old;
            else 
                if (fillZero) 
                    this._pixels[0] = 0;
        }

        public void ScrollDown(int deviceIndex = 0)
        {
            int offset = deviceIndex * MATRIX_ROW_SIZE;
            for (var i = 0; i < MATRIX_ROW_SIZE-1; i++)
            {
                if(deviceIndex < 0 || deviceIndex >= MatrixCount)
                    return;
                _pixels[offset + i] = (byte)(_pixels[offset + i + 1]);
            }
            _pixels[offset + MATRIX_ROW_SIZE - 1] = 0;
        }

        public void ScrollUp(int deviceIndex = 0)
        {
            if(deviceIndex < 0 || deviceIndex >= MatrixCount) return;
            int offset = deviceIndex * MATRIX_ROW_SIZE;
            for (var i = MATRIX_ROW_SIZE-1; i > 0 ; i--)
            {
                _pixels[offset + i] = (byte)(_pixels[offset + i - 1]);
            }
            _pixels[offset + 0] = 0;
        }
        
        public void WriteSprite(int deviceIndex, int x, int y, int spriteIndex, List<byte> sprite)
        {
            spriteIndex = spriteIndex * 7; // 7 size of sprint data

	        int w = sprite[spriteIndex];
	        int h = sprite[spriteIndex+1];

            if (h == 8 && y == 0)
            {
                for (int i = 0; i < w; i++)
                {
                    int c = x + i;
                    if (c >= 0 && c < 80)
                        SetColumn(deviceIndex, c, sprite[spriteIndex + i + 2]);
                }
            }
            else
            {
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        int c = x + i;
                        int r = y + j;
                        if (c >= 0 && c < 80 && r >= 0 && r < 8)
                            SetLed(deviceIndex, c, r, BitUtil.IsSet((int) sprite[spriteIndex + i + 2], (byte) j));
                    }
                }
            }
        }

        public void ScrollLeft(int deviceIndex = 0)
        {
            for(var i = 0; i < MATRIX_ROW_SIZE; i++)
                ScrollLeftColumn(deviceIndex, i);
        }

        public void ScrollRight(int deviceIndex = 0)
        {
            for(var i = 0; i < MATRIX_ROW_SIZE; i++)
                ScrollRightColumn(deviceIndex, i);
        }

        public void ScrollRightColumn(int deviceIndex, int row)
        {
            if(deviceIndex < 0 || deviceIndex >= MatrixCount)
                return;
            int offset = deviceIndex * MATRIX_COL_SIZE;
            _pixels[offset + row] = (byte)(_pixels[offset + row] << 1);
        }

        public void ScrollLeftColumn(int deviceIndex, int row)
        {
            if(deviceIndex < 0 || deviceIndex >= MatrixCount)
                return;
            int offset = deviceIndex * MATRIX_COL_SIZE;
            _pixels[offset + row] = (byte)(_pixels[offset + row] >> 1);
        }

    }
}
