/*
    Demo application for the NusbioMatrix MCU based.
    Support 1, 4, 8 Matrix.

    Copyright (C) 2016 MadeInTheUSB LLC
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
            Assembly currentAssem = typeof (Program).Assembly;
            object[] attribs = currentAssem.GetCustomAttributes(typeof (AssemblyCopyrightAttribute), true);
            if (attribs.Length > 0)
                return ((AssemblyCopyrightAttribute) attribs[0]).Copyright;
            return null;
        }

        static string GetAssemblyProduct()
        {
            Assembly currentAssem = typeof (Program).Assembly;
            object[] attribs = currentAssem.GetCustomAttributes(typeof (AssemblyProductAttribute), true);
            if (attribs.Length > 0)
                return ((AssemblyProductAttribute) attribs[0]).Product;
            return null;
        }

        private static void CheckKeyboard(ref bool quit, ref int speed)
        {
            if (Console.KeyAvailable)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.Q:
                        quit = true;
                        break;
                    case ConsoleKey.Add:
                    case ConsoleKey.OemPlus:
                        speed += 10;
                        break;
                    case ConsoleKey.Subtract:
                    case ConsoleKey.OemMinus:
                        speed -= 10;
                        if (speed < 0) speed = 0;
                        break;
                }
            }
        }


        
        private static void ReadDemo(NusbioEEPROM nusbioEEPROM)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Read Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");

            var r = nusbioEEPROM.SetAddress(129);
            r = nusbioEEPROM.Read(1);

            var k = Console.ReadKey(true);
        }

        static void Cls(NusbioEEPROM nusbioEEPROM)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(-1, 2, "0) Read all   W)rite All ");
            ConsoleEx.WriteMenu(-1, 4, "I)nit device  Q)uit");

            var m = string.Format("Firmware {0} v {1}, Port:{2}, EEPROM Size:{3}kb Page:{4}", 
                nusbioEEPROM.Firmware, nusbioEEPROM.FirmwareVersion, nusbioEEPROM.ComPort,
                nusbioEEPROM.EepromInfo.Size, nusbioEEPROM.EepromInfo.PageCount, nusbioEEPROM.EepromInfo.PageSize);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 3, m, ConsoleColor.White, ConsoleColor.DarkCyan);
        }

        private static NusbioEEPROM ConnectToMCU(NusbioEEPROM nusbioPixel)
        {
            if (nusbioPixel != null)
            {
                nusbioPixel.Dispose();
                nusbioPixel = null;
            }
            var comPort = new NusbioEEPROM().DetectMcuComPort(Mcu.FirmwareName.NusbioMcuEeprom);
            if (comPort == null)
            {
                Console.WriteLine("Nusbio EEPROM not detected");
                return null;
            }
            nusbioPixel = new NusbioEEPROM(comPort);
            if (nusbioPixel.Initialize().Succeeded)
            {
                return nusbioPixel;
            }
            return null;
        }

        static void Main(string[] args)
        {
            var quit    = false;
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct());
            Console.WriteLine("");

            NusbioEEPROM nusbioEEPROM = ConnectToMCU(null);            
            if (nusbioEEPROM == null) return;
            Cls(nusbioEEPROM);

            while (!quit)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;

                    if (k == ConsoleKey.D0) ReadDemo(nusbioEEPROM);
                    if (k == ConsoleKey.Q) quit = true;

                    if (k == ConsoleKey.I)
                    {
                        nusbioEEPROM = ConnectToMCU(nusbioEEPROM).Wait(500);
                    }
                    Cls(nusbioEEPROM);
                }
            }
            nusbioEEPROM.Dispose();
        }
    }
}

