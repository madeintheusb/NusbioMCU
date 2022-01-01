//#define _300_LEDS
/*
    Demo application for the NusbioMCU and Multi-Color LED (RGB, WS2812)
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
            
            ConsoleEx.WriteMenu(-1, 2, "0) Rainbow all strip demo  1) Rainbow spread demo  S)quare demo  L)ine demo  8)x8 Square Demo");
            ConsoleEx.WriteMenu(-1, 6, "I)nit device  Q)uit");

            var m = string.Format("Firmware {0} v {1}, Port:{2}, LED Count:{3}", 
                nusbioMatrix.Firmware, 
                nusbioMatrix.FirmwareVersion,
                nusbioMatrix.ComPort,
                nusbioMatrix.Count);

            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 3, m, ConsoleColor.White, ConsoleColor.DarkCyan);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
        }
        
        private static string ToHexValue(Color color)
        {
            return $"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}";
        }

        enum RainbowEffect
        {
            AllStrip,
            Spread
        }

        private static void Square8x8Demo(NusbioPixel nusbioPixel)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Square 8x8 Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");

            var quit         = false;
            var speed        = 75;
            var jStep        = 4;
            var j            = 0;
            var color        = RGBHelper.Wheel(j);
            var rgb8x8Matrix = new Matrix8x8(nusbioPixel);

            rgb8x8Matrix.Clear(Color.Black).Show().Wait(speed);

            while (!quit)
            {
                for (var x = 0; x < 4; x++)
                {
                    color = RGBHelper.Wheel(j);
                    j    += jStep;
                    if (j >= 256) j = 0;

                    rgb8x8Matrix.DrawRect(new Rectangle(x, x, 8-(x*2), 8-(x*2)), color);
                    rgb8x8Matrix.Show().Wait(speed);
                }

                rgb8x8Matrix.Wait(speed);
                CheckKeyboard(ref quit, ref speed);
                if (quit) break;

                // Draw the matrix to all black
                for (var x = 4-1; x >= 0; x--)
                {
                    rgb8x8Matrix.DrawRect(new Rectangle(x, x, 8 - (x * 2), 8 - (x * 2)), Color.Black);
                    rgb8x8Matrix.Show().Wait(speed);
                }

                rgb8x8Matrix.Wait(speed);
                CheckKeyboard(ref quit, ref speed);
                if (quit) break;

                ConsoleEx.Write(0, 23, $"Speed:{speed}", ConsoleColor.Cyan);
                ConsoleEx.Write(0, 24, nusbioPixel.GetByteSecondSentStatus(true), ConsoleColor.Cyan);
            }
        }

        private static void Square4x4Demo(NusbioPixel nusbioPixel)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Square 4x4 Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");

            int speed = nusbioPixel.Count <= 16 ? 32 : 16;
            var quit  = false;
            var jStep = 32;
            Color bkColor = Color.Black;

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
            int speed               = nusbioPixel.Count <= 30 ? 5 : 00;
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
                    ConsoleEx.Write(0, 22, string.Format("show() {0} times for animation time:{1:000}ms {2:000}, {3}", halfLedCount, sw.ElapsedMilliseconds, sw.ElapsedMilliseconds / halfLedCount, nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.Cyan);
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
            return d1 + (d2 - d1) * fraction;
        }

        private static void RainbowDemo(NusbioPixel nusbioPixel, RainbowEffect rainbowEffect, int speed = 1, int jWheelColorStep = 4, int brightness = 64)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Rainbow Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");
            var quit                  = false;
            
            nusbioPixel.SetBrightness(brightness);
            if (nusbioPixel.Firmware == Mcu.FirmwareName.NusbioMcu2StripPixels)
                nusbioPixel.SetBrightness(brightness, NusbioPixel.StripIndex.S1);

            if (nusbioPixel.Firmware == Mcu.FirmwareName.NusbioMcu2StripPixels)
                speed /= 2;

            while (!quit)
            {
                for (var jWheelColorIndex = 0; jWheelColorIndex < 256; jWheelColorIndex += jWheelColorStep)
                {
                    ConsoleEx.WriteLine(0, 2, string.Format("jWheelColorIndex:{0:000}, jWheelColorStep:{1:00}", jWheelColorIndex, jWheelColorStep), ConsoleColor.White);
                    var sw = Stopwatch.StartNew();

                    for (var pixelIndex = 0; pixelIndex < nusbioPixel.Count; pixelIndex++)
                    {
                        var color       = Color.Beige;

                        if (rainbowEffect == RainbowEffect.AllStrip)
                            color = RGBHelper.Wheel((jWheelColorIndex) & 255);
                        else if(rainbowEffect == RainbowEffect.Spread)
                            color = RGBHelper.Wheel((pixelIndex * 256 / nusbioPixel.Count) + jWheelColorIndex);

                        if(pixelIndex == 0) // Setting the pixel this way, will support more than 255 LED
                            nusbioPixel.SetPixel(pixelIndex, color); // Set led index to 0
                        else
                            nusbioPixel.SetPixel(color); // Set led to current position

                        // With NusbioMCU with 2 string, set the second string
                        if (nusbioPixel.Firmware == Mcu.FirmwareName.NusbioMcu2StripPixels && nusbioPixel.Count <= NusbioPixel.NUSBIO_PIXELS_MCU_MAX_LED)
                        {
                            if (pixelIndex == 0)
                                nusbioPixel.SetPixel(pixelIndex, color, NusbioPixel.StripIndex.S1);
                            else
                                nusbioPixel.SetPixel(color, NusbioPixel.StripIndex.S1);
                        }

                        if (nusbioPixel.Count <= 120)
                        {
                            if (pixelIndex % 4 == 0) Console.WriteLine();
                            Console.Write("[{0:x2}]rgb:{1:x2},{2:x2},{3:x2} ", pixelIndex, color.R, color.G, color.B);
                        }
                    }

                    nusbioPixel.Show();
                    if (nusbioPixel.Firmware == Mcu.FirmwareName.NusbioMcu2StripPixels)
                        nusbioPixel.Show(NusbioPixel.StripIndex.S1);

                    sw.Stop();

                    ConsoleEx.Write(0, 23, string.Format("SetPixel()/Show() {0}", nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.Cyan);
                    
                    if (speed > 0) Thread.Sleep(speed);
                    CheckKeyboard(ref quit, ref speed);
                    if (quit)   
                        break;
                }
            }
        }

        static NusbioPixelDeviceType AskUserForPixelType()
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct());
            var m = "Pixel Type:  Strip 3)0  Strip 6)0  S)quare 16  R)ing 12 Strip 1)80  Square 8)x8";
#if _300_LEDS
            m += " 3 H)undred";
#endif
            var pixelTypeChar = ConsoleEx.Question(1,
                m, new List<char>() {'3', '6', 'S', 'R', '1', '8'
                        #if _300_LEDS
                            , 'H'
                        #endif
                });
            return GetMaxLed(pixelTypeChar);
        }

        private static NusbioPixelDeviceType GetMaxLed(char pixelTypeChar)
        {
            switch (pixelTypeChar)
            {
                case '3': return NusbioPixelDeviceType.Strip30;
                case '8': return NusbioPixelDeviceType.Square8x8;
                case '6': return NusbioPixelDeviceType.Strip60;
                case 'S': return NusbioPixelDeviceType.Square16;
                case 'R': return NusbioPixelDeviceType.Ring12;
                case 'H': return NusbioPixelDeviceType.Strip300;
                case '1': return NusbioPixelDeviceType.Strip180;
            }
            return NusbioPixelDeviceType.Unknown;
        }

        static string GetArg(string[] args, string name)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == name)
                    return args[i + 1];
            }
            return null;
        }

        static int GetArg(string[] args, string name, int defaultValue)
        {
            var a = GetArg(args, name);
            if (a == null)
                return defaultValue;
            return int.Parse(a);
        }

        /// <summary>
        /// -deviceType 8 8x8LedSquare -command 48 -speed 512 -jWheelColorStep 1 -brightness 12
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var quit = false;

            if (args.Length > 0)
            {
                var devType = GetArg(args, "-deviceType")[0];
                _rgbLedType = GetMaxLed(devType);
            }
            else {
                _rgbLedType = AskUserForPixelType();
            }
            var MAX_LED                 = (int)_rgbLedType;
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct());
            Console.WriteLine("");

            NusbioPixel nusbioPixel = NusbioPixel.ConnectToMCU(null, maxLED);
            if (nusbioPixel == null) return;

            Cls(nusbioPixel);
            

            while (!quit)
            {
                ConsoleKey k = ConsoleKey.Escape;
                ConsoleEx.WaitMS(256);
                if (Console.KeyAvailable)
                {
                    k = Console.ReadKey(true).Key;
                }
                else {
                    var c = GetArg(args, "-command", 0);
                    if (c > 0)
                        k = (ConsoleKey)c;
                }
                if (k == ConsoleKey.Q) quit = true;

                if (k == ConsoleKey.D0) RainbowDemo(nusbioPixel, RainbowEffect.AllStrip, GetArg(args, "-speed", 1), GetArg(args, "-jWheelColorStep", 4), GetArg(args, "-brightness", 64));
                if (k == ConsoleKey.D1) RainbowDemo(nusbioPixel, RainbowEffect.Spread, GetArg(args, "-speed", 1), GetArg(args, "-jWheelColorStep", 4), GetArg(args, "-brightness", 64));
                if (k == ConsoleKey.S) SquareDemo(nusbioPixel);
                if (k == ConsoleKey.L) LineDemo(nusbioPixel);

                if (k == ConsoleKey.I)
                {
                    nusbioPixel = NusbioPixel.ConnectToMCU(nusbioPixel, MAX_LED).Wait(500).SetStrip(Color.Green);
                }
                if(k != ConsoleKey.Escape)
                {
                    Cls(nusbioPixel);
                }
                
            }
            nusbioPixel.Dispose();
        }
    }
}


