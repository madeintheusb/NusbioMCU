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
    public partial class NusbioEEPROM : NusbioMCU, IDisposable
    {
        public class EEPROM_INFO : McuComResponse {
            public int PageSize { get;set; }
            public int Size{ get;set; }

            public int PageCount
            {
                get
                {
                    return (this.Size * 1024) / this.PageSize;
                }
            }
        }

        public EEPROM_INFO EepromInfo;

        public NusbioEEPROM(int baud = BAUD) : base(null, baud)
        {
            this._baud = baud;
        }

        public NusbioEEPROM(string comPort, int baud = BAUD) : base(comPort, baud)
        {
            this._comPort = comPort;
            this._baud = baud;
            this.ResetBytePerSecondCounters();
        }

        /// <summary>
        /// EEPROM Firmware has never been finished
        /// </summary>
        /// <param name="firmwareNames"></param>
        /// <returns></returns>
        public override McuComResponse Initialize(List<Mcu.FirmwareName> firmwareNames = null)
        {
            if (firmwareNames == null)
                firmwareNames = new List<Mcu.FirmwareName>();

            if (firmwareNames.Count == 0)
                firmwareNames.Add(Mcu.FirmwareName.NusbioMcuEeprom);

            var r = base.Initialize(firmwareNames);
            if (r.Succeeded)
            {
                var eepromInfo = this.GetInfo();
                if(eepromInfo.Succeeded)
                    return eepromInfo;
                return r;
            }
            return r;
        }

        private NusbioEEPROM CheckBreak(McuComResponse r)
        {
            if ((!r.Succeeded) && Debugger.IsAttached)
                Debugger.Break();
            return this;
        }

        public NusbioEEPROM Wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
            return this;
        }

        public void Dispose()
        {
            base.Close();
        }

        public McuComResponse Read(int pageCount)
        {
            this.Send(Mcu.McuCommand.CP_EEPROM_READ_PAGES, pageCount);
            var r = ReadAnswer(256+3);
            base.CleanBuffer();
            return r;
        }

        public McuComResponse SetAddress(int addr16bit)
        {
            ValidateAddress(addr16bit);

            this.Send(Mcu.McuCommand.CP_EEPROM_SET_ADDR, addr16bit >> 8, addr16bit & 0xFF);
            var r = ReadAnswer();
            if (r.Succeeded)
                return r;
            else
                return r;
        }

        private void ValidateAddress(int addr16bit)
        {
            if (addr16bit < 0 || addr16bit > this.EepromInfo.PageCount)
                throw new ArgumentException(string.Format("Invalid address:{0}", addr16bit));
        }

        public EEPROM_INFO GetInfo()
        {
            this.EepromInfo = new EEPROM_INFO();
            Send(Mcu.McuCommand.CP_EEPROM_GET_INFO, 0);
            var r = ReadAnswer(6);
            if (r.Succeeded)
            {
                this.EepromInfo.Succeeded    = true;
                this.EepromInfo.Size     = r.GetParam(0) * 4;
                this.EepromInfo.PageSize = r.GetParam(1) * 4;
            }
            return this.EepromInfo;
        }
    }
}
