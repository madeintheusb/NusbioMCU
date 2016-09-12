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

        static void Cls(FutabaLCD  nusbioMatrix)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(-1, 2, "0) Speed Test");
            ConsoleEx.WriteMenu(-1, 4, "I)nit device  Q)uit");

            var maxtrixCount = nusbioMatrix.Count;
            var m = string.Format("Firmware {0} v {1}, Port:{2}", nusbioMatrix.Firmware, nusbioMatrix.FirmwareVersion, nusbioMatrix.ComPort);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 3, m, ConsoleColor.White, ConsoleColor.DarkCyan);
        }

        private static FutabaLCD  ConnectToMCU(FutabaLCD  lcd)
        {
            if (lcd != null)
            {
                lcd.Dispose();
                lcd = null;
            }
            var comPort = new FutabaLCD().DetectMcuComPort( Mcu.FirmwareName.FutabaLCD);
            if (comPort == null)
            {
                Console.WriteLine("Futaba Driver not detected");
                return null;
            }
            lcd = new FutabaLCD(comPort);
            if (lcd.Initialize( Mcu.FirmwareName.FutabaLCD).Succeeded)
            {
                return lcd;
            }
            return null;
        }
        

        static void Main(string[] args)
        {
            var quit    = false;
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct());
            Console.WriteLine("");
            FutabaLCD futabaLCD = ConnectToMCU(null);
            if (futabaLCD == null) return;
            Cls(futabaLCD);

            while (!quit)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q) quit = true;
                    if (k == ConsoleKey.D0)
                        futabaLCD.RequestTest();
                    
                    if (k == ConsoleKey.I)
                    {
                        futabaLCD = ConnectToMCU(futabaLCD).Wait(500);
                    }
                    Cls(futabaLCD);
                }
            }
            futabaLCD.Dispose();
        }
    }
}

