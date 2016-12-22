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
            
            ConsoleEx.WriteMenu(-1, 3, "D)emo");
            ConsoleEx.WriteMenu(-1, 6, "I)nit device  Q)uit");

            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
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
      
        private static void ArcadeDemo(NusbioPixel nusbioPixel, RainbowEffect rainbowEffect, Mcu.GpioPwmPin pwnPinForWhiteStrip)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct());
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");

            var quit                = false;
            var speed               = 20;
            var jWheelColorStep     = 4;

            nusbioPixel.SetBrightness(64*2);

            var whiteStripPWMIntensityIndex = 0;
            var whiteStripPWMIntensity = new List<int>()
            {
                0, 32, 128, 228
            };

            while (!quit)
            {
                // Control the intensitity of the white strip
                var r = nusbioPixel.AnalogWrite(pwnPinForWhiteStrip, whiteStripPWMIntensity[whiteStripPWMIntensityIndex++]);
                ConsoleEx.WriteLine(0, 2, string.Format("WhiteStrip Intensity(PWM):{0:000}", whiteStripPWMIntensity[whiteStripPWMIntensityIndex - 1]), ConsoleColor.White);
                if (whiteStripPWMIntensityIndex >= whiteStripPWMIntensity.Count)
                    whiteStripPWMIntensityIndex = 0;

                // Animate the 2 30 LED strip in sync
                for (var jWheelColorIndex = 0; jWheelColorIndex < 256; jWheelColorIndex += jWheelColorStep)
                {
                    ConsoleEx.WriteLine(0, 4, string.Format("jWheelColorIndex:{0:000}, jWheelColorStep:{1:00}", jWheelColorIndex, jWheelColorStep), ConsoleColor.White);

                    var sw = Stopwatch.StartNew();

                    var halfLedCount = nusbioPixel.Count / 2;

                    for (var i = 0; i < halfLedCount; i++)
                    {
                        var color = Color.Beige;

                        if (rainbowEffect == RainbowEffect.AllStrip)
                            color = RGBHelper.Wheel((i+jWheelColorIndex) & 255);
                        else if(rainbowEffect == RainbowEffect.Spread)
                            color = RGBHelper.Wheel((i * 256 / nusbioPixel.Count) + jWheelColorIndex);
                        
                        nusbioPixel.SetPixel(i, color.R, color.G, color.B); // Set led index to 0
                        nusbioPixel.SetPixel(i+ halfLedCount, color.R, color.G, color.B); // Set led index to 0

                        if (i%4 == 0) Console.WriteLine();

                        Console.Write("[{0:x2}]rgb:{1:x2},{2:x2},{3:x2} ", i, color.R, color.G, color.B); // , ToHexValue(color) html value
                    }
                    nusbioPixel.Show();
                    sw.Stop();
                    ConsoleEx.Write(0, 22, string.Format("SetPixel()/Show() Time:{0:000}ms, {1}", sw.ElapsedMilliseconds, nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.Cyan);

                    if (speed > 0)
                        Thread.Sleep(speed);
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
            _rgbLedType                 = NusbioPixelDeviceType.Strip60;
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
                    if (k == ConsoleKey.D)
                        ArcadeDemo(nusbioPixel, RainbowEffect.Spread, Mcu.GpioPwmPin.Gpio5);

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


