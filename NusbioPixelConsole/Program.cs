/*
    Demo application for the NusbioMCU and Multi-Color LED (RGB, WS2812)
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

        static void Cls(NusbioPixel nusbioMatrix)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(-1, 2, "0) Rainbow all strip demo  1) Rainbow spread demo  S)quare demo");
            ConsoleEx.WriteMenu(-1, 4, "I)nit device  Q)uit");

            var maxtrixCount = nusbioMatrix.Count;
            var m = string.Format("Firmware {0} v {1}, Port:{2}, LED Count:{3}", nusbioMatrix.Firmware, nusbioMatrix.FirmwareVersion, nusbioMatrix.ComPort, maxtrixCount);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 3, m, ConsoleColor.White, ConsoleColor.DarkCyan);
        }

        private static NusbioPixel ConnectToMCU(NusbioPixel nusbioPixel, int maxLed)
        {
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
                if (nusbioPixel.SetBrightness(nusbioPixel.DEFAULT_BRIGHTNESS).Succeeded)
                {
                    return nusbioPixel;
                }
            }
            return null;
        }
        
        private static string ToHexValue(Color color)
        {
            return "#" + color.R.ToString("X2") +
                   color.G.ToString("X2") +
                   color.B.ToString("X2");
        }

        enum RainbowEffect
        {
            AllStrip,
            Spread
        }


        private static void SquareDemo(NusbioPixel nusbioPixel)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Square 8x8 Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");

            int speed = nusbioPixel.Count <= 16 ? 32 : 16;
            var quit  = false;
            var jStep = 32;
            Color bkColor;

            while (!quit)
            {
                for (var j = 0; j < 256; j += jStep)
                {
                    ConsoleEx.WriteLine(0, 2, string.Format("jStep:{0:000}", j), ConsoleColor.White);

                    for (var i = 0; i < nusbioPixel.Count; i++)
                    {
                        if (i == 0 || true)
                        {
                            bkColor = RGBHelper.Wheel((i+j) & 255);
                            nusbioPixel.SetStrip(bkColor).Show();
                        }

                        var newColor = Color.FromArgb(bkColor.B, bkColor.G, bkColor.R);
                        nusbioPixel.SetPixel(i, newColor);
                        nusbioPixel.Show();
                        if (speed > 0) Thread.Sleep(speed);
                        CheckKeyboard(ref quit, ref speed);
                        if (quit) break;
                    }
                    if (speed > 0)
                        Thread.Sleep(speed*2);
                    if (quit) break;
                }
                ConsoleEx.Write(0, 24, nusbioPixel.GetByteSecondSentStatus(true), ConsoleColor.Cyan);
            }
        }

        private static void RainbowDemo(NusbioPixel nusbioPixel, RainbowEffect rainbowEffect)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Rainbow Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");

            nusbioPixel.SetBrightness(64);

            int speed = nusbioPixel.Count <= 16 ? 10 : 2;

            var quit  = false;
            var jStep = 4;

            while (!quit)
            {
                for (var j = 0; j < 256; j += jStep)
                {
                    ConsoleEx.WriteLine(0, 2, string.Format("jStep:{0:000}", j), ConsoleColor.White);

                    var sw = Stopwatch.StartNew();

                    for (var i = 0; i < nusbioPixel.Count; i++)
                    {
                        var color = Color.Beige;

                        if (rainbowEffect == RainbowEffect.AllStrip)
                            color = RGBHelper.Wheel((i+j) & 255);
                        else if(rainbowEffect == RainbowEffect.Spread)
                            color = RGBHelper.Wheel((i * 256 / nusbioPixel.Count) + j);
                        
                        if (i == 0)
                            nusbioPixel.SetPixel(i, color.R, color.G, color.B); // Set led index to 0
                        else
                            nusbioPixel.SetPixel(color.R, color.G, color.B);

                        if (i%4 == 0) Console.WriteLine();

                        Console.Write("[{0:x2}]rgb:{1:x2},{2:x2},{3:x2} ", i, color.R, color.G, color.B); // , ToHexValue(color) html value
                    }
                    sw.Stop();
                    ConsoleEx.Write(0, 23, string.Format("SetPixel Time:{0:000}ms", sw.ElapsedMilliseconds), ConsoleColor.Cyan);
                    nusbioPixel.Show();
                    if (speed > 0)
                        Thread.Sleep(speed);
                    CheckKeyboard(ref quit, ref speed);
                    if (quit)
                        break;
                }
                ConsoleEx.Write(0, 24, nusbioPixel.GetByteSecondSentStatus(true), ConsoleColor.Cyan);
            }
        }

        private static void RingDemo(NusbioPixel nusbioPixel, int deviceIndex = 0)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Rainbow Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");
            int speed = 0;
            var quit  = false;
            var jStep = 4;

            nusbioPixel.SetBrightness(24);

            while (!quit)
            {
                for (jStep = 2; jStep < 24; jStep += 2)
                {
                    ConsoleEx.WriteLine(0, 2, string.Format("jStep:{0:000}", jStep), ConsoleColor.White);

                    for (var j = 0; j < 256; j += jStep)
                    {
                        for (var i = 0; i < nusbioPixel.Count; i++)
                        {
                            var color = RGBHelper.Wheel((i * 256 / nusbioPixel.Count) + j);
                            if (i == 0)
                                nusbioPixel.SetPixel(i, color.R, color.G, color.B);
                            else
                                nusbioPixel.SetPixel(color.R, color.G, color.B);

                            //Console.Write("[{0:000}]rgb:{1:000},{2:000},{3:000}  html:{4} ", i, color.R, color.G, color.B, ToHexValue(color));
                            //if (i%2 != 0) Console.WriteLine();
                        }
                        nusbioPixel.Show();
                        speed = jStep / 2;
                        if (speed > 0)
                            Thread.Sleep(speed);
                        CheckKeyboard(ref quit, ref speed);
                        if (quit)
                            break;
                    }
                }


                for (jStep = 24; jStep > 2; jStep -= 2)
                {
                    ConsoleEx.WriteLine(0, 2, string.Format("jStep:{0:000}", jStep), ConsoleColor.White);

                    for (var j = 256; j > 0; j -= jStep)
                    {
                        for (var i = 0; i < nusbioPixel.Count; i++)
                        {
                            var color = RGBHelper.Wheel((i * 256 / nusbioPixel.Count) + j);
                            if (i == 0)
                                nusbioPixel.SetPixel(i, color.R, color.G, color.B);
                            else
                                nusbioPixel.SetPixel(color.R, color.G, color.B);

                            //Console.Write("[{0:000}]rgb:{1:000},{2:000},{3:000}  html:{4} ", i, r.R, r.G, r.B, ToHexValue(r));
                            //if (i%2 != 0) Console.WriteLine();
                        }
                        nusbioPixel.Show();
                        speed = jStep / 2;
                        if (speed > 0)
                            Thread.Sleep(speed);
                        CheckKeyboard(ref quit, ref speed);
                        if (quit)
                            break;
                    }
                }
                ConsoleEx.WriteMenu(0, 4, nusbioPixel.GetByteSecondSentStatus(true));
            }
        }

        static int AskUserForLedCount()
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct());
            var ledCountChar = ConsoleEx.Question(1, "RG LED Count  8)  1)0  3)0  6)0  S)quare 16  R)ing 12", new List<char>() {'1', '3', '6', 'S', '8', 'R'});
            switch (ledCountChar)
            {
                case '1': return 10;
                case '3': return 30;
                case '4': return 44;
                case '6': return 60;
                case '8': return  8;
                case 'S': return 16;
                case 'R': return 12;
            }
            return 0;
        }

        static void Main(string[] args)
        {
            var quit    = false;
            var MAX_LED = AskUserForLedCount();
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct());
            Console.WriteLine("");

            NusbioPixel nusbioPixel = ConnectToMCU(null, MAX_LED);            
            if (nusbioPixel == null) return;
            nusbioPixel.SetStrip(Color.Green);
            Cls(nusbioPixel);

            while (!quit)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q) quit = true;
                    if (k == ConsoleKey.D0) RainbowDemo(nusbioPixel, RainbowEffect.AllStrip);
                    if (k == ConsoleKey.D1) RainbowDemo(nusbioPixel, RainbowEffect.Spread);
                    if (k == ConsoleKey.S) SquareDemo(nusbioPixel);

                    if (k == ConsoleKey.I)
                    {
                        nusbioPixel = ConnectToMCU(nusbioPixel, MAX_LED).Wait(500).SetStrip(Color.Green);
                    }
                    Cls(nusbioPixel);
                }
            }
            nusbioPixel.Dispose();
        }
    }
}

