
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace MadeInTheUSB.Communication
{
    public interface IMcuCom
    {
        string PortName  { get; }
        int BaudRate     { get; }
        
        bool IsConnected{ get; }

        void Send(byte[] buffer);
        byte[] ReadBuffer(int len);
    }

    public class McuComBaseClass
    {
        public static List<string> GetMcuPortName(int indexStart = 3)
        {
            var allPorts = SerialPort.GetPortNames().ToList();
            var l = new List<string>();

            for (var i = indexStart; i < 300; i++)
            {
                var p = string.Format("COM{0}", i);
                if (allPorts.Contains(p))
                    l.Add(p);
            }
            return l;
        }   
    } 
    /// <summary>
    /// Handle serial communication with the Nusbio Matrix Firmware
    /// https://social.msdn.microsoft.com/Forums/en-US/7423c81f-987f-4975-9382-1289b45f65d4/working-with-systemioports-to-send-and-receive-data-accurately?forum=csharplanguage
    /// http://stackoverflow.com/questions/35870794/serialport-basestream-readasync-missing-the-first-byte
    /// </summary>
    public class McuCom : McuComBaseClass, IDisposable, IMcuCom
    {
        const int MAX_RESPONSE_BUFFER = 16;

        internal SerialPort _serialPort;

        public string PortName  { get; set; }
        public int BaudRate     { get; set; }

        public Queue<List<byte>> ReceivedBuffers = new Queue<List<byte>>();

        public McuCom(string portName, int baud)
        {
            this.BaudRate       = baud;
            this.PortName       = portName;
            this._serialPort    = new SerialPort(portName);

            this._serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(port_ErrorReceived);
            this._serialPort.DataReceived  += new SerialDataReceivedEventHandler(port_DataReceived);

            if (this.Open())
            {
                this.CleanBuffers();
            }
            else
            {
                throw new ApplicationException(string.Format("Cannot open port:{0}", portName));
            }
        }

        private void CleanBuffers()
        {
            var r = this._serialPort.BytesToRead;
            if (r > 0)
            {
                var tmpBuffer = new byte[MAX_RESPONSE_BUFFER];
                if (this._serialPort.BytesToRead > MAX_RESPONSE_BUFFER)
                {
                    tmpBuffer = new byte[MAX_RESPONSE_BUFFER*4];
                    var read2 = this._serialPort.Read(tmpBuffer, 0, this._serialPort.BytesToRead);
                    throw new ArgumentException();
                }
                var read = this._serialPort.Read(tmpBuffer, 0, this._serialPort.BytesToRead);
            }
            this._serialPort.DiscardInBuffer();
            this._serialPort.DiscardOutBuffer();
            this.ReceivedBuffers.Clear();
        }

        private bool Open(int retryCount = 5, int waitTime = 2000)
        {
            int count = 0;
            while (count < retryCount)
            {
                try
                {
                    this._serialPort.BaudRate  = this.BaudRate;
                    this._serialPort.Parity    = Parity.None;
                    this._serialPort.StopBits  = StopBits.One;
                    this._serialPort.RtsEnable = false;
                    this._serialPort.DtrEnable = false; // Do not auto reset
                    
                    //var hs = this._serialPort.Handshake;
                    //var d = this._serialPort.DataBits;
                    //var s = this._serialPort.NewLine;
                    //var size = this._serialPort.ReadBufferSize;
                    //var timeout = this._serialPort.ReadTimeout;
                    count++;
                    this._serialPort.Open();
                    return true;
                }
                catch (System.Exception)
                {
                    Thread.Sleep(waitTime);
                }
            }
            return false;
        } 

        //public /*async*/ void SendOptimize(byte [] buffer)
        //{
        //    try {
        //        //this.CleanBuffers();
        //        this._serialPort.Write(buffer, 0, buffer.Length);
        //        //this._serialPort.BaseStream.WriteAsync(buffer, 0, buffer.Length);
        //        //byte[] rBuffer = new byte[3];
        //        //await this._serialPort.BaseStream.ReadAsync(rBuffer, 0, 3);
        //        //ReceivedBuffers.Enqueue(rBuffer.ToList());
        //    }
        //    catch(System.Exception ex)
        //    {
        //        throw new ApplicationException("Communication error", ex);
        //    }

        //    const int blockLimit = 3;
        //    byte[] rrbuffer = new byte[blockLimit];
        //    Action kickoffRead = null;
        //    kickoffRead = delegate
        //    {
        //        this._serialPort.BaseStream.BeginRead(rrbuffer, 0, rrbuffer.Length,
        //            delegate (IAsyncResult ar)
        //            {
        //                try
        //                {
        //                    int actualLength = this._serialPort.BaseStream.EndRead(ar);
        //                    byte[] received = new byte[actualLength];
        //                    Buffer.BlockCopy(rrbuffer, 0, received, 0, actualLength);
        //                    ReceivedBuffers.Enqueue(received.ToList());
        //                    this._serialPort.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                            
        //                }
        //                catch (Exception exc)
        //                {
        //                }
        //                //kickoffRead();
        //            }, null);
        //    };
        //    kickoffRead();
        //    Thread.Sleep(10);
        //    while (ReceivedBuffers.Count > 1)
        //        ReceivedBuffers.Dequeue();

        //}

        public byte[] ReadBuffer(int len)
        {
            var sleepTime       = 1;
            var timeOutCounter  = 0;
            while (timeOutCounter < 6)
            {
                Thread.Sleep(sleepTime);
                if (this.ReceivedBuffers.Count > 0)
                {
                    var buffer = this.ReceivedBuffers.Dequeue();
                    return buffer.ToArray();
                }
                timeOutCounter++;
                if (sleepTime < 4)
                    sleepTime++;
            }
            return null;
        }

        public void Send(byte [] buffer)
        {
            try {
                this._serialPort.Write(buffer, 0, buffer.Length);
            }
            catch(System.Exception ex)
            {
                throw new ApplicationException("Communication error", ex);
            }
        }

        public bool IsConnected
        {
            get { return _serialPort.IsOpen; }
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var tmpBuffer = new byte[MAX_RESPONSE_BUFFER];
                if (this._serialPort.BytesToRead > 0)
                {
                    if (this._serialPort.BytesToRead > MAX_RESPONSE_BUFFER)
                    {
                        var tmpBuffer2 = new byte[1024];
                        var read = this._serialPort.Read(tmpBuffer2, 0, this._serialPort.BytesToRead);
                        if (Debugger.IsAttached) Debugger.Break();
                        this.CleanBuffers();
                        throw new ArgumentException(string.Format(" NusbioMatrix MCU response too large ({0} bytes)", this._serialPort.BytesToRead));
                    }

                    var bytesToRead = this._serialPort.BytesToRead;
                    if (bytesToRead >= 3)
                    {
                        var read = this._serialPort.Read(tmpBuffer, 0, this._serialPort.BytesToRead);
                        ReceivedBuffers.Enqueue(tmpBuffer.Take(read).ToList());
                    }
                    else
                    {
                        //if (Debugger.IsAttached) Debugger.Break();
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        void port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine("COM Port Error Event:{0}", e.EventType);
        }
        
        public void CloseConnection()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }

        public virtual void Dispose()
        {
            CloseConnection();
        }
    }
}
