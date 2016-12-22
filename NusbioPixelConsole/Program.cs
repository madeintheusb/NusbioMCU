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
        static NusbioPixelDeviceType _rgbLedType;

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

        private static bool CheckKeyboard(ref bool quit, ref int speed)
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
            return quit;
        }

        static void Cls(NusbioPixel nusbioMatrix)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            
            ConsoleEx.WriteMenu(-1, 2, "0) Rainbow all strip demo  1) Rainbow spread demo  S)quare demo  L)ine demo");
            ConsoleEx.WriteMenu(-1, 4, "C)hristmas Colors");
            ConsoleEx.WriteMenu(-1, 6, "I)nit device  Q)uit");

            //var maxtrixCount = nusbioMatrix.Count;
            //var m = string.Format("Firmware {0} v {1}, Port:{2}, LED Count:{3}", nusbioMatrix.Firmware, nusbioMatrix.FirmwareVersion, nusbioMatrix.ComPort, maxtrixCount);
            //ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 3, m, ConsoleColor.White, ConsoleColor.DarkCyan);
            //ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
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

        private static void LineDemo(NusbioPixel nusbioPixel)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Line Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");

            long maxShowPerformance = 0;
            long minShowPerformance = long.MaxValue;

            nusbioPixel.SetBrightness(nusbioPixel.DEFAULT_BRIGHTNESS);

            var quit                = false;
            int speed               = nusbioPixel.Count < 30 ? 75 : 0;
            var wheelColorMaxStep   = 256;
            var jWheelColorStep     = 4;
            Color color             = Color.Beige;

            while (!quit)
            {
                for (var jWheelColorIndex = 0; jWheelColorIndex < wheelColorMaxStep; jWheelColorIndex += jWheelColorStep)
                {
                    // Set the background color of the strip
                    ConsoleEx.WriteLine(0, 2, string.Format("jWheelColorIndex:{0:000}, jWheelColorStep:{1:000}, Speed:{2:000}", jWheelColorIndex, jWheelColorStep, speed), ConsoleColor.White);
                    var sw = Stopwatch.StartNew();
                    color  = RGBHelper.Wheel((jWheelColorIndex) & 255);
                    for (var i = 0; i < nusbioPixel.Count; i++) // Set all te pixel to one color
                    {
                        if (i == 0)
                            nusbioPixel.SetPixel(i, color); // Set led index to 0
                        else
                            nusbioPixel.SetPixel(color);

                        if (i % 4 == 0) Console.WriteLine();
                        Console.Write("[{0:x2}]rgb:{1:x2},{2:x2},{3:x2} ", i, color.R, color.G, color.B); // , ToHexValue(color) html value
                    }
                    sw.Stop();
                    ConsoleEx.Write(0, 20, string.Format("SetPixel() Time:{0:000}ms, {1}", sw.ElapsedMilliseconds, nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.Cyan);
                    sw = Stopwatch.StartNew();
                    nusbioPixel.Show();
                    sw.Stop();
                    if (sw.ElapsedMilliseconds < minShowPerformance) minShowPerformance = sw.ElapsedMilliseconds;
                    if (sw.ElapsedMilliseconds > maxShowPerformance) maxShowPerformance = sw.ElapsedMilliseconds;
                    ConsoleEx.Write(0, 21, string.Format("Show() Time:{0:000}ms max:{1:000} min:{2:000}, {3}", sw.ElapsedMilliseconds, maxShowPerformance, minShowPerformance, nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.Cyan);

                    var halfLedCount = (nusbioPixel.Count / 2) + 1;
                    // Set the foreground Color or animation color scrolling
                    sw = Stopwatch.StartNew();
                    var fgColor = RGBHelper.Wheel((jWheelColorIndex+(((int)(jWheelColorStep*4)))) & 255);
                    for (var i = 0; i < halfLedCount; i++)
                    {
                        nusbioPixel.SetPixel(i, fgColor);
                        nusbioPixel.SetPixel(nusbioPixel.Count-i, fgColor);
                        nusbioPixel.Show();
                        if (speed > 0)
                            Thread.Sleep(speed); // Give a better effect
                        if(CheckKeyboard(ref quit, ref speed))
                            break;
                    }
                    sw.Stop();
                    ConsoleEx.Write(0, 22, string.Format("show() {0} times for animation time:{1:000}ms {2:000}, {3}",
                        halfLedCount,
                        sw.ElapsedMilliseconds,
                        sw.ElapsedMilliseconds / halfLedCount,
                        nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.Cyan);
                    
                    if (speed > 0)
                        Thread.Sleep(speed);
                    if(CheckKeyboard(ref quit, ref speed) || quit)
                        break;
                }
            }
        }

        public static Color GetBlendedColor(int percentage)
        {
            if (percentage < 50)
                return Interpolate(Color.Red, Color.Yellow, percentage / 50.0);
            return Interpolate(Color.Yellow, Color.Green, (percentage - 50) / 50.0);
        }

        private static Color Interpolate(Color color1, Color color2, double fraction)
        {
            double r = Interpolate(color1.R, color2.R, fraction);
            double g = Interpolate(color1.G, color2.G, fraction);
            double b = Interpolate(color1.B, color2.B, fraction);
            return Color.FromArgb((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b));
        }

        private static double Interpolate(double d1, double d2, double fraction)
        {
            //return d1 + (d1 - d2) * fraction;
            return d1 + (d2 - d1) * fraction;
        }

       

      
        private static void RainbowDemo(NusbioPixel nusbioPixel, RainbowEffect rainbowEffect)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Rainbow Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");

            long maxShowPerformance = 0;
            long minShowPerformance = long.MaxValue;
            nusbioPixel.SetBrightness(64);

            var quit  = false;
            int speed = nusbioPixel.Count <= 16 ? 10 : 0;
            var jWheelColorStep = 4;

            nusbioPixel.SetBrightness(64*4);
            speed = 50;

            while (!quit)
            {
                for (var jWheelColorIndex = 0; jWheelColorIndex < 256; jWheelColorIndex += jWheelColorStep)
                {
                    ConsoleEx.WriteLine(0, 2, string.Format("jWheelColorIndex:{0:000}, jWheelColorStep:{1:00}", jWheelColorIndex, jWheelColorStep), ConsoleColor.White);

                    var sw = Stopwatch.StartNew();

                    for (var i = 0; i < nusbioPixel.Count; i++)
                    {
                        var color = Color.Beige;

                        if (rainbowEffect == RainbowEffect.AllStrip)
                            color = RGBHelper.Wheel((i+jWheelColorIndex) & 255);
                        else if(rainbowEffect == RainbowEffect.Spread)
                            color = RGBHelper.Wheel((i * 256 / nusbioPixel.Count) + jWheelColorIndex);
                        
                        if (i == 0)
                            nusbioPixel.SetPixel(i, color.R, color.G, color.B); // Set led index to 0
                        else
                            nusbioPixel.SetPixel(color.R, color.G, color.B);

                        if (i%4 == 0) Console.WriteLine();

                        Console.Write("[{0:x2}]rgb:{1:x2},{2:x2},{3:x2} ", i, color.R, color.G, color.B); // , ToHexValue(color) html value
                    }
                    sw.Stop();
                    ConsoleEx.Write(0, 22, string.Format("SetPixel() Time:{0:000}ms, {1}", sw.ElapsedMilliseconds, nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.Cyan);

                    sw = Stopwatch.StartNew();
                    nusbioPixel.Show();
                    sw.Stop();
                    
                    if (sw.ElapsedMilliseconds < minShowPerformance) minShowPerformance = sw.ElapsedMilliseconds;
                    if (sw.ElapsedMilliseconds > maxShowPerformance) maxShowPerformance = sw.ElapsedMilliseconds;

                    ConsoleEx.Write(0, 23, string.Format("Show() Time:{0:000}ms max:{1:000} min:{2:000}, {3}", 
                        sw.ElapsedMilliseconds,
                        maxShowPerformance,
                        minShowPerformance,
                        nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.Cyan);

                    if (speed > 0)
                        Thread.Sleep(speed);
                    CheckKeyboard(ref quit, ref speed);
                    if (quit)
                        break;
                }
                
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

        static NusbioPixelDeviceType AskUserForPixelType()
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct());
            var pixelTypeChar = ConsoleEx.Question(1, "Pixel Type:  Strip 3)0  Strip 6)0  S)quare 16  R)ing 12", new List<char>() {'3', '6', 'S', 'R'});
            switch (pixelTypeChar)
            {
                case '3': return NusbioPixelDeviceType.Strip30;
                case '6': return NusbioPixelDeviceType.Strip60;
                case 'S': return NusbioPixelDeviceType.Square16;
                case 'R': return NusbioPixelDeviceType.Ring12;
            }
            return NusbioPixelDeviceType.Unknown;
        }

        static void Main(string[] args)
        {
            var quit                    = false;
             _rgbLedType                = AskUserForPixelType();
            var MAX_LED                 = (int)_rgbLedType;
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
                    if (k == ConsoleKey.L) LineDemo(nusbioPixel);

                    if (k == ConsoleKey.I)
                    {
                        nusbioPixel = ConnectToMCU(nusbioPixel, MAX_LED).Wait(500).SetStrip(Color.Green);
                    }
                    Cls(nusbioPixel);
                }
                else ConsoleEx.WaitMS(100);
            }
            nusbioPixel.Dispose();
        }
    }
}


