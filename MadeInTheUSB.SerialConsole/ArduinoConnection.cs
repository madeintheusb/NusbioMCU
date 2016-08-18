using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace ArduinoLibrary
{
    /// <summary>
    /// provides very low level access to the serial _serialPort that an ardiuno is plugged in to
    /// </summary>
    public class ArduinoConnection : IDisposable 
    {
        SerialPort _serialPort;
        private StringBuilder _textReceived = new StringBuilder(1024);

        public string PortName  { get; set; }
        public int BaudRate     { get; set; }
        public Queue<String> ReceivedMessages = new Queue<string>();

        public ArduinoConnection(string portName, int baud)
        {
            this.BaudRate       = baud;
            _serialPort         = new SerialPort(portName);
            this.PortName       = portName;
            _serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(port_ErrorReceived);
            _serialPort.DataReceived  += new SerialDataReceivedEventHandler(port_DataReceived);

            if (this.Open())
            {
                this._serialPort.DiscardInBuffer();
                this._serialPort.DiscardOutBuffer();
            }
            else
            {
                throw new ApplicationException(string.Format("Cannot open port:{0}", portName));
            }
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
                    this._serialPort.RtsEnable = true;
                    this._serialPort.DtrEnable = true;
                    
                    var d = this._serialPort.DataBits;
                    var s = this._serialPort.NewLine;
                    var size = this._serialPort.ReadBufferSize;
                    var timeout = this._serialPort.ReadTimeout;
                    this._serialPort.ReadTimeout = 2000;

                    this._serialPort.Open();
                    return true;
                }
                catch (System.Exception ex)
                {
                    Thread.Sleep(waitTime);
                }
            }
            return false;
        } 

        public void Send(string text)
        {
            this._serialPort.WriteLine(text);
        }

        public bool IsConnected
        {
            get { return _serialPort.IsOpen; }
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //var t = this._serialPort.ReadExisting();
            //while (t.Contains("\n"))
            //{
            //    var pos = t.IndexOf("\n");
            //    var s1  = t.Substring(0, pos - 1);
            //    ReceivedMessages.Enqueue(s1);
            //    t = t.Substring(pos + 1);
            //}

            var s = this._serialPort.ReadExisting();
            //Console.WriteLine("ReadExisting:{0}", s);
            _textReceived.Append(s);

            if (_textReceived.ToString().EndsWith("\n"))
            {
                var text = _textReceived.ToString().Replace("\n", "");
                ReceivedMessages.Enqueue(text);
                _textReceived.Clear();
            }
        }

        void port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine("COM Port Error Event:{0}", e.EventType);
        }

        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
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
