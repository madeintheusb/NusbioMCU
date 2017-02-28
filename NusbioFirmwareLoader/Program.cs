/*
    NusbioFirmwareLoader
    Copyright (C) 2016,2017 MadeInTheUSB LLC
    Written by FT
    
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
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadeInTheUSB;
using System.Drawing;
using MadeInTheUSB.MCU;
using MadeInTheUSB.Components;

namespace NusbioMatrixConsole
{
    class Program
    {
        public static string GetAssemblyCopyright()
        {
            Assembly currentAssem = typeof(Program).Assembly;
            object[] attribs = currentAssem.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
            if (attribs.Length > 0)
                return ((AssemblyCopyrightAttribute)attribs[0]).Copyright;
            return null;
        }

        static string GetAssemblyProduct()
        {
            Assembly currentAssem = typeof(Program).Assembly;
            object[] attribs = currentAssem.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if (attribs.Length > 0)
                return ((AssemblyProductAttribute)attribs[0]).Product;
            return null;
        }

        static void Cls()
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(-1, 2, "U)pload firmware");
            ConsoleEx.WriteMenu(-1, 4, "Q)uit");
            ConsoleEx.WriteMenu(-1, 20, string.Format("ARDUINO_IDE:{0}", ARDUINO_IDE));

            //var m = string.Format("Firmware {0} v {1}, Port:{2}", nusbioMatrix.Firmware, nusbioMatrix.FirmwareVersion, nusbioMatrix.ComPort);
            var m = "";
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 3, m, ConsoleColor.White, ConsoleColor.DarkCyan);
        }

        private static NusbioPixel ConnectToMCU(NusbioPixel nusbioPixel, int maxLed)
        {
            Console.WriteLine("Detecting Nusbio...");
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
                return nusbioPixel;
            }
            return null;
        }

        public static string ARDUINO_IDE = @"C:\DVT\Arduino\Arduino-1.6.9";

        static string LastFirmwareFile
        {
            get
            {
                var consolePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(consolePath, @"Firmware\Last\NusbioMatrixATMega328.ino.hex");
            }
        } 

        static bool UploadFirmware(string comPort) {

            Console.Clear();
            var yesNo = ConsoleEx.Question(1, string.Format("Upload firmware to Nusbio COM:{0} Y)es N)o", comPort), new List<char>() { 'Y', 'N' });
            if (yesNo == 'Y')
            {
                var u = new ArduinoFirmwareUploader(ARDUINO_IDE, MCU_TYPE.atmega328p, comPort, LastFirmwareFile);
                if(u.Upload())
                {
                    Console.WriteLine("Upload succeeded");
                }
                else
                {
                    Console.WriteLine("Upload failed");
                }
            }
            Console.WriteLine("Hit any key to continue");
            var k = Console.ReadKey();
            return true;
        }

        static void Main(string[] args)
        {
            var quit = false;
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct());
            Console.WriteLine("");

            string comPort = "";
            NusbioPixel nusbioPixel = ConnectToMCU(null, NusbioPixel.DEFAULT_PIXEL_COUNT);
            if (nusbioPixel != null)
            {
                comPort = nusbioPixel.ComPort;
                nusbioPixel.SetStrip(Color.Green);
                nusbioPixel.Dispose();
            }
            else
            {
                Console.WriteLine();
                Console.Write("Enter NusbioMCU COM port (COMX)?");
                comPort = Console.ReadLine();

                Console.Write("Arduino IDE Path ({0})?", ARDUINO_IDE);
                var tmpArduinoIDE = Console.ReadLine();
                if (tmpArduinoIDE == "")
                    tmpArduinoIDE = ARDUINO_IDE;
            }
            Cls();

            while (!quit)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q) quit = true;
                    if (k == ConsoleKey.U) UploadFirmware(comPort);

                    Cls();
                }
                else ConsoleEx.WaitMS(100);
            }
        }
    }
}
