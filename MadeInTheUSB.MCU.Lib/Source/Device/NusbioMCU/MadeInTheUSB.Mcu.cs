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

namespace MadeInTheUSB.MCU
{
    public enum PowerMode
    {
        USB = 1,
        EXTERNAL = 2
    }
    public partial class NusbioMCU
    {
        /// <summary>
        /// Default serial communication speed
        /// </summary>
        public const int BAUD = 115200;//115200;  // 115200 57600 128000 230400

        protected McuCom    _mcu;
        protected string    _comPort;
        protected int       _baud;

        /// <summary>
        /// If the RGB LED pixel or the 8x8 LED matrix is powered via NusbioMCU
        /// the current os drawn from the USB. USB amp consumption is limited to 500 mA.
        /// </summary>
        public PowerMode PowerMode = PowerMode.USB;

        public Mcu.FirmwareName Firmware;
        public int FirmwareVersion;

        public string Port { get { return this._comPort; } }

        public NusbioMCUProtocolRecorder Recorder = new NusbioMCUProtocolRecorder();

        public NusbioMCU(string comPort = null, int baud = BAUD) 
        {
            this._comPort   = comPort;
            this._baud      = baud;
            this.ResetBytePerSecondCounters();
        }

        public void CleanBuffer()
        {
            _mcu.CleanBuffers();
        }

        public string DetectMcuComPort(Mcu.FirmwareName expectedFirmware = Mcu.FirmwareName.NusbioMcuMatrixPixel, int reTry = 5)
        {
            int tryCounter = 0;
            int waitTime = 1000;
            while (tryCounter < reTry)
            {
                var comPort = __DetectMcuComPort(expectedFirmware);
                if (comPort != null)
                    return comPort;
                tryCounter++;
                Thread.Sleep(waitTime);
                waitTime += 1250;
            }
            return null;
        }

        private string __DetectMcuComPort(Mcu.FirmwareName expectedFirmware )
        {
            var ports = McuCom.GetMcuPortName();
            foreach (var p in ports)
            {
                try
                {
                    Console.Write(".");
                    this._mcu = new McuCom(p, BAUD);
                    if (this.Ping().Succeeded) // This will turn on the on board led
                    {
                        this.Firmware = this.DetectFirmware();
                        if (this.Firmware == expectedFirmware)
                            return p;
                    }
                }
                finally
                {
                    if (this._mcu != null)
                    {
                        this._mcu.Dispose();
                        this._mcu = null;
                    }
                }
            }
            return null;
        }

        protected McuComResponse InitializationDone()
        {
            return this.SetOnBoardLed(OnBoardLedMode.Connected);
        }

        public virtual McuComResponse Initialize(Mcu.FirmwareName firmwareName = Mcu.FirmwareName.NusbioMcuMatrixPixel)
        {
            this._mcu = new McuCom(this._comPort, this._baud);
            var r = this.Ping();
            if (r.Succeeded)
            {
                this.FirmwareVersion = r.GetParam(1);
                this.Firmware        = this.DetectFirmware();
                if (this.Firmware == firmwareName)
                {
                    this.SetOnBoardLed(OnBoardLedMode.Connected);
                    return McuComResponse.Success;
                }
                else
                    return new McuComResponse().Fail(string.Format("Invalid firmware:{0}", this.Firmware));
            }
            return new McuComResponse().Fail("Firmware not found");
        }

        public virtual void Close()
        {
            this.SetOnBoardLed(OnBoardLedMode.Disconnected);
            _mcu.CloseConnection();
        }

        /// <summary>
        /// Wait up to 100 ms for an answer from the NusbioMatrix MCU Firmware
        /// </summary>
        /// <returns></returns>
        internal McuComResponse ReadAnswer(int expectedSize = 3, int minimumWait = 1)
        {
            var r = new McuComResponse();
            var buffer = this._mcu.ReadBuffer(expectedSize, minimumWait);
            if (buffer != null)
            {
                this.Recorder.AddReceived(buffer.ToList());
                return r.Initialize(buffer.ToList());
            }
            else
                return r.Fail("Timeout");
        }

        public string ComPort
        {
            get { return _mcu.PortName; }
        }

        private string BufferToCode(byte[] buffer)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            for (var i = 0; i < buffer.Length; i++)
            {
                sb.AppendFormat("{0},", buffer[i]);
            }
            var s = sb.ToString();
            s = s.Substring(0, s.Length - 1) + "}";
            return s;
        }

        private void SendBuffer(byte[] buffer)
        {
            this.Recorder.AddSent(buffer.ToList());
            this.AddByte(buffer.Length);
            _mcu.Send(buffer);
            //System.Diagnostics.Debug.WriteLine(BufferToCode(buffer));
        }

        public void Send(Mcu.McuCommand api, params byte[] buffer)
        {
            var bb = new byte[buffer.Length + 1];
            bb[0]  = (byte) api;
            for (var i = 0; i < buffer.Length; i++)
                bb[i + 1] = buffer[i];
            this.SendBuffer(bb);
        }

        public void Send(Mcu.McuCommand api, int param0, params byte[] buffer)
        {
            var bb = new byte[buffer.Length + 2];
            bb[0]  = (byte) api;
            bb[1]  = (byte) param0;
            for (var i = 0; i < buffer.Length; i++)
                bb[i + 2] = buffer[i];
            this.SendBuffer(bb);
        }

        public void Send(Mcu.McuCommand api, int param0, int param1, params byte[] buffer)
        {
            var bb = new byte[buffer.Length + 3];
            bb[0] = (byte) api;
            bb[1] = (byte) param0;
            bb[2] = (byte) param1;
            for (var i = 0; i < buffer.Length; i++)
                bb[i + 3] = buffer[i];
            this.SendBuffer(bb);
        }

        public McuComResponse TestSimulator()
        {
            this.Send(Mcu.McuCommand.CP_TEST_SIMULATOR);
            var r = ReadAnswer();
            return r;
        }

        public static List<Mcu.AdcPin> AllAdcs = new List<Mcu.AdcPin>()
        {
            Mcu.AdcPin.Adc4, Mcu.AdcPin.Adc5, Mcu.AdcPin.Adc6, Mcu.AdcPin.Adc7
        };

        public int AnalogRead(Mcu.AdcPin port)
        {
            this.Send(Mcu.McuCommand.CP_ANALOG_READ, (int)port);
            var r    = this.ReadAnswer();
            r.Values = new List<int>();
            if (r.Succeeded && r.Buffer.Count == 2)
                return (r.Buffer[0] << 8) + r.Buffer[1];
            else
                return -1;
        }

        public bool DigitalRead(Mcu.GpioPin port)
        {
            this.Send(Mcu.McuCommand.CP_DIGITAL_READ, (int)port);
            var r    = this.ReadAnswer();
            r.Values = new List<int>();
            if (r.Succeeded && r.Buffer.Count == 2) // Answer is always 2 byte
                return r.Buffer[0] == 1;
            else
                throw new InvalidOperationException();
        }

        public static List<Mcu.GpioPin> AllGpios = new List<Mcu.GpioPin>()
        {
            Mcu.GpioPin.Gpio4, Mcu.GpioPin.Gpio5, Mcu.GpioPin.Gpio6, Mcu.GpioPin.Gpio7,
        };

        public bool AnalogWrite(Mcu.GpioPwmPin pin, int value)
        {
            if (value < 0)
                value = 0;
            this.Send(Mcu.McuCommand.CP_ANALOG_WRITE, (int)pin, value);
            var r = ReadAnswer();
            return r.Succeeded && r.GetParam(0) == (int)pin && r.GetParam(1) == value;
        }

        public bool DigitalWrite(Mcu.GpioPin port, bool state)
        {
            var intState = state ? 1 : 0;
            this.Send(Mcu.McuCommand.CP_DIGITAL_WRITE, (int)port, intState);
            return true;
        }

        public bool SetDigitalPinMode(Mcu.GpioPin port, Mcu.DigitalIOMode mode)
        {
            this.Send(Mcu.McuCommand.CP_SET_PIN_MODE, (int)port, (int)mode);
            var r    = this.ReadAnswer();
            r.Values = new List<int>();
            if (r.Succeeded && r.Buffer.Count == 2) // Answer is always 2 byte
                return r.Buffer[0] == (int)port;
            else
                return false;
        }

        public enum OnBoardLedMode
        {
            Off             = 0,
            On              = 1,
            Disconnected    = 150, // will be multiplied by 10
            Connected       = 75, // will be multiplied by 10
        }

        public McuComResponse SetOnBoardLed(OnBoardLedMode mode)
        {
            Send(Mcu.McuCommand.CP_ONBOAD_LED, (int)mode);
            var r = ReadAnswer();
            return r;
        }

        public McuComResponse Ping()
        {
            // PING Activate in firmare the non blinking LED
            // mode, we should call the api from C$ rather
            Send(Mcu.McuCommand.CP_PING);
            var r = ReadAnswer();
            if (r.Succeeded)
            {
                r.BoardValidation = r.GetParam(0);
            }
            return r;
        }

        public Mcu.FirmwareName DetectFirmware()
        {
            Send(Mcu.McuCommand.CP_GETFIRMWARE_NAME);
            var r = ReadAnswer();
            if (r.Succeeded)
            {
                var firmwareName = (Mcu.FirmwareName)r.GetParam(0);
                return firmwareName;
            }
            return Mcu.FirmwareName.Unknown;
        }
    }
}
