/*
    MadeInTheUSB.ArduinoFirmwareUploader
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
using System.IO;
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
    public enum MCU_TYPE
    {
        atmega328p,
    }
        
    public partial class ArduinoFirmwareUploader
    {
        string _arduinoIde;
        MCU_TYPE _mcuType;
        string _comPort;
        string _firmWare;

        public ArduinoFirmwareUploader(string arduinoIde, MCU_TYPE mcuType, string comPort, string firmWare)
        {
            this._arduinoIde = arduinoIde;
            this._mcuType = mcuType;
            this._comPort = comPort;
            this._firmWare = firmWare;
        }

        public bool Upload()
        {
            int exitCode = -1;
            Console.WriteLine("");
            Console.WriteLine("Command Line:");
            Console.WriteLine("{0} {1}", this.GetAvrdudeExe(), this.GetCommandLine());
            if(ExecProgram(this.GetAvrdudeExe(), this.GetCommandLine(), true, ref exitCode, false, false))
            {
                return exitCode == 0;
            }
            return false;
        }

        private string GetCommandLine()
        {
            // echo "%avrdude%" -C%avrdude_conf% -v -patmega328p -carduino -P%COMPORT%
            // -b57600 -D -Uflash:w:%FIRMWARE%:i
            var sb = new StringBuilder();

            sb.AppendFormat("-C{0} ", this.GetAvrdudeConfFile());
            sb.Append("-v ");
            sb.AppendFormat("-p{0} ", this._mcuType);
            sb.Append("-carduino ");
            sb.AppendFormat("-P{0} ", this._comPort);
            sb.Append("-b57600 ");
            sb.Append("-D ");
            sb.AppendFormat("-Uflash:w:{0}:i", this._firmWare);

            return sb.ToString();
        }

        private string GetAvrdudeConfFile()
        {
            return Path.Combine(this._arduinoIde, @"hardware\tools\avr\etc\avrdude.conf");
        }
        private string GetAvrdudeExe()
        {
            return Path.Combine(this._arduinoIde, @"hardware\tools\avr\bin\avrdude.exe");
        }

        static bool ExecProgram(string strProgram, string strParameter, bool booWait, ref int intExitCode, bool booSameProcess, bool booHidden)
        {
            try
            {
                System.Diagnostics.Process proc;

                if (booSameProcess)
                {
                    proc = System.Diagnostics.Process.GetCurrentProcess();
                }
                else
                {
                    proc = new System.Diagnostics.Process();
                }
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = strProgram;
                proc.StartInfo.Arguments = strParameter;

                if (booHidden)
                {
                    proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                }

                proc.Start();


                if (booWait)
                {
                    proc.WaitForExit();
                    intExitCode = proc.ExitCode;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}