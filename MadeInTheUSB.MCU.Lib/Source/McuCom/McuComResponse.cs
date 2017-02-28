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
using System.Collections.Generic;
using System.Text;

namespace MadeInTheUSB.Communication
{
    public class McuComResponse
    {
        public bool Succeeded;
        public List<byte> Buffer;
        public string Error;
        public int BoardValidation = -1;
        public List<int> Values;

        public static McuComResponse Success = new McuComResponse() { Succeeded = true };

        public override string ToString()
        {
            if (this.Succeeded)
            {
                return string.Format("Succeeded:{0} ", this.Succeeded);
            }
            else
            {
                var s = "";
                s += string.Format("Succeeded:{0} ", this.Succeeded);
                if (Buffer.Count > 0)
                {
                    var b = new StringBuilder();
                    foreach (var by in Buffer)
                        b.AppendFormat("{0} ", by);    
                    s += string.Format("Buffer:[{0}] ", b);
                }
                if (this.Error != null)
                {
                    s += string.Format("Error:{0} ", this.Error);
                }
                return s;
            }
        }

        public enum ApiErrorCode : byte
        {
            CP_OK       = 128,
            CP_FAILED   = 64
        }

        public int GetParam(int index)
        {
            if (index >= this.Buffer.Count)
            {
                if(System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                return -1;
            }
            else
                return this.Buffer[index];
        }

        public McuComResponse()
        {
            
        }

        public McuComResponse Initialize(List<byte> buffer)
        {
            Succeeded = buffer[0] == (byte)ApiErrorCode.CP_OK;
            buffer.RemoveAt(0);
            this.Buffer = new List<byte>();
            this.Buffer.AddRange(buffer);
            return this;
        }

        public McuComResponse Timeout()
        {
            this.Error = "timeout";
            this.Succeeded = false;
            return this;
        }

        public McuComResponse Fail(string error)
        {
            this.Error = error;
            this.Succeeded = false;
            return this;
        }
    }
}