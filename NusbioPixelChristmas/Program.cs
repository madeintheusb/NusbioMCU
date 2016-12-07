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
            
            ConsoleEx.WriteMenu(-1, 2, "C)hristmas Colors   T)ree");
            ConsoleEx.WriteMenu(-1, 6, "I)nit device   Q)uit");

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

        /// <summary>
        /// http://stackoverflow.com/questions/2011832/generate-color-gradient-in-c-sharp
        /// </summary>
        /// <param name="nusbioPixel"></param>
        private static void ChristmasColorDemo(NusbioPixel nusbioPixel)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Christmas Color Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");
            nusbioPixel.SetBrightness(nusbioPixel.DEFAULT_BRIGHTNESS);

            var quit               = false;
            int speed              = 5;
            var sourceColor        = Color.Green;
            var targetColor        = Color.Red;
            var currentTargetColor = Color.Red;
            var currentColor       = sourceColor;
            var step               = 2;
            var sw                 = Stopwatch.StartNew();

            while (!quit)
            {
                ConsoleEx.Write(0, 2, string.Format("Current Color RGB {0:000}, {1:000}, {2:000}", currentColor.R, currentColor.G, currentColor.B), ConsoleColor.Cyan);
                ConsoleEx.Write(0, 3, string.Format("Target Color  RGB {0:000}, {1:000}, {2:000}", targetColor.R, targetColor.G, targetColor.B), ConsoleColor.Cyan);
                
                for (var i = 0; i < nusbioPixel.Count; i++)
                    nusbioPixel.SetPixel(i, currentColor);

                nusbioPixel.Show();
                currentColor = RGBHelper.MoveToColor(currentColor, currentTargetColor, step);

                if(RGBHelper.Equal(currentColor, currentTargetColor))
                {
                    sw.Stop();
                    ConsoleEx.Write(0, 5, string.Format("Time:{0:000}ms {1}", sw.ElapsedMilliseconds, nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.Cyan);
                    currentTargetColor = RGBHelper.Equal(currentColor, Color.Red) ? sourceColor : targetColor;
                    sw = Stopwatch.StartNew();
                }
                if (speed > 0) Thread.Sleep(speed);
                if (CheckKeyboard(ref quit, ref speed) || quit)
                    break;
            }
        }

        public static void AnimateSquareXY(Square4x4 square4x4, List<Point> points, int speed, Color destColor, Color sourceColor) {
            
            points.All(p => square4x4.SetPixel(p.X, p.Y, destColor, refresh: true, wait: speed, interruptOnKeyboard: false));
            Thread.Sleep(speed);
            points.Reverse();
            points.All(p => square4x4.SetPixel(p.X, p.Y, sourceColor, refresh: true, wait: speed, interruptOnKeyboard: false));
            points.Reverse();
        }

        public static bool FadeInFadeOutOneTreeLight(Color treeColor, Square4x4 square4x4, List<Color> gradiantColors, Point pt, int wait)
        {
            gradiantColors.All(p => square4x4.SetPixel(pt.X, pt.Y, p, true, wait));
            gradiantColors.Reverse();
            if (Console.KeyAvailable) return false;
            Thread.Sleep(wait * 3);

            var lastColor = gradiantColors[gradiantColors.Count - 1];
            gradiantColors[gradiantColors.Count - 1] = treeColor;
            gradiantColors.All(p => square4x4.SetPixel(pt.X, pt.Y, p, true, wait));
            gradiantColors[gradiantColors.Count - 1] = lastColor;
            gradiantColors.Reverse();
            if (Console.KeyAvailable) return false;
            return true;        
        }

        static bool FadeInFadeOutMultipleTreeLight(
            Color treeColor,
            Square4x4 square4x4,
            Dictionary<Point, Color> treeDefinition,
            List<Color> gradiantColors, int wait)
        {
            foreach (var c1 in gradiantColors)
            {
                treeDefinition.All( p => square4x4.SetPixel(p.Key.X, p.Key.Y, treeDefinition[p.Key] == Color.Red ? c1 : treeDefinition[p.Key]) );
                if (Console.KeyAvailable) return false;
                square4x4.Show(wait);
            }

            gradiantColors.Reverse();
            var lastColor = gradiantColors[gradiantColors.Count - 1];
            gradiantColors[gradiantColors.Count - 1] = treeColor;

            foreach (var c1 in gradiantColors)
            {
                treeDefinition.All(p => square4x4.SetPixel(p.Key.X, p.Key.Y, treeDefinition[p.Key] == Color.Red ? c1 : treeDefinition[p.Key]));
                if (Console.KeyAvailable) return false;
                square4x4.Show(wait);
            }
            gradiantColors[gradiantColors.Count - 1] = lastColor;
            gradiantColors.Reverse();
            return true;
        }

        private static void ChristmasTreeDemo(NusbioPixel nusbioPixel)
        {
            try
            {
                Console.Clear();
                ConsoleEx.TitleBar(0, "Christmas Tree Demo");
                ConsoleEx.WriteMenu(-1, 8, "Q)uit");

                var treeColor                   = Color.FromArgb(0, 0, 64, 2);
                var skyColor                    = Color.DarkBlue;
                var blueGreen                   = Color.FromArgb(0, 0, 104, 34);
                var quit                        = false;
                var speed                       = 100;
                var randomNumber                = new Random(Guid.NewGuid().GetHashCode());
                var square4x4                   = new Square4x4(nusbioPixel);
                var jWheelIndexMin              = 64;
                var jWheelIndexMax              = 160;
                var jWheelIndex                 = randomNumber.Next(jWheelIndexMin, jWheelIndexMax);

                nusbioPixel.SetBrightness(192);

                // Draw the tree
                var greenTreeDefinition = new Dictionary<Point, Color>()
                {
                    { new Point(0, 0), treeColor },
                    { new Point(0, 1), treeColor },
                    { new Point(0, 2), treeColor },
                    { new Point(1, 1), treeColor },
                    { new Point(2, 1), treeColor },
                    { new Point(1, 0), treeColor },
                    { new Point(1, 2), treeColor },
                };

                var targetColorForAnimation = new List<Color>()
                {
                    //Color.Red,
                    //Color.OrangeRed,
                    //Color.DarkSalmon,
                    //Color.DarkOrange,
                    //Color.DarkRed,
                    //Color.Yellow,
                    //Color.Sienna,
                    Color.Crimson,
                    Color.Chocolate,
                    Color.Gainsboro,
                    Color.LavenderBlush,
                    Color.Khaki,
                    Color.LemonChiffon,
                    Color.LightGray,
                    Color.LightYellow,
                    Color.LimeGreen
                };
                var targetColorForAnimationIndex = 0;

                // Tree with 3 extremities red
                var greenTreeDefinitionWithRedLights = new Dictionary<Point, Color>()
                {
                    { new Point(0, 0), Color.Red }, // The red color are used are marker, they will be replaced on the fly by random color
                    { new Point(0, 1), treeColor },
                    { new Point(0, 2), Color.Red },
                    { new Point(1, 1), treeColor },
                    { new Point(2, 1), Color.Red },
                    { new Point(1, 0), treeColor },
                    { new Point(1, 2), treeColor },
                };

                nusbioPixel.SetPixel(0, Color.Black, nusbioPixel.Count); // Clear all

                AnimateSquareXY(square4x4, square4x4.GetPoints(), 0, Color.DarkTurquoise, skyColor);
                nusbioPixel.SetPixel(0, Color.Black, nusbioPixel.Count); // Clear all
                nusbioPixel.SetPixel(0, skyColor, nusbioPixel.Count-4, refresh: true); // Display one the first 3 columns in blue

                while (!quit)
                {
                    var sw = Stopwatch.StartNew();

                    if (_rgbLedType != NusbioPixelDeviceType.Square16) // For all device but the square set all led to blue
                        nusbioPixel.SetPixel(0, skyColor, nusbioPixel.Count, refresh: true);

                    if (CheckKeyboard(ref quit, ref speed) || quit)
                        break;
                    else
                        if (speed > 0) Thread.Sleep(speed);
                    
                    square4x4.Drawpoints(greenTreeDefinition, refresh: true, wait: speed); // Draw tree in green

                    var sparkColor = RGBHelper.Wheel(jWheelIndex);
                    var gradiantColorFromGreenToSparkColors = RGBHelper.MoveToColors(treeColor, sparkColor, 10);
                    jWheelIndex = randomNumber.Next(jWheelIndexMin, jWheelIndexMax);

                    ConsoleEx.WriteLine(0, 2, string.Format("jWheelIndex:{0:000}, Spark Color:[{1:X2},{2:X2},{3:X2}]", jWheelIndex, sparkColor.R, sparkColor.G, sparkColor.B), ConsoleColor.DarkYellow);

                    targetColorForAnimationIndex += 1;
                    if (targetColorForAnimationIndex == targetColorForAnimation.Count)
                        targetColorForAnimationIndex = 0;

                    for (var c = 0; c < 3; c++)
                    {
                        if (!FadeInFadeOutOneTreeLight(treeColor, square4x4, gradiantColorFromGreenToSparkColors,
                            greenTreeDefinition.Keys.ToList()[randomNumber.Next(0, greenTreeDefinition.Keys.Count - 1)], randomNumber.Next(speed / 16, speed / 12)))
                            break;
                        Thread.Sleep(randomNumber.Next(speed/2, speed));
                    }

                    if (randomNumber.Next(0, 5) == 3)
                        FadeInFadeOutMultipleTreeLight(treeColor, square4x4, 
                            greenTreeDefinitionWithRedLights,
                            gradiantColorFromGreenToSparkColors, randomNumber.Next(speed / 16, speed / 12));

                    sw.Stop();
                    ConsoleEx.Write(0, 4, string.Format("SetPixel() Time:{0:000}ms, {1}", sw.ElapsedMilliseconds, nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.DarkYellow);
                    Thread.Sleep(randomNumber.Next(0, speed*3));
                }
            }
            finally
            {
                nusbioPixel.SetPixel(0, Color.Green, nusbioPixel.Count, refresh: true);
                nusbioPixel.SetBrightness(nusbioPixel.DEFAULT_BRIGHTNESS);
            }
        }

        static NusbioPixelDeviceType AskUserForPixelType()
        {
            return NusbioPixelDeviceType.Square16;
            //Console.Clear();
            //ConsoleEx.TitleBar(0, GetAssemblyProduct());
            //var pixelTypeChar = ConsoleEx.Question(1, "Pixel Type:  Strip 3)0  Strip 6)0  S)quare 16  R)ing 12", new List<char>() {'3', '6', 'S', 'R'});
            //switch (pixelTypeChar)
            //{
            //    case '3': return NusbioPixelDeviceType.Strip30;
            //    case '6': return NusbioPixelDeviceType.Strip60;
            //    case 'S': return NusbioPixelDeviceType.Square16;
            //    case 'R': return NusbioPixelDeviceType.Ring12;
            //}
            //return NusbioPixelDeviceType.Unknown;
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

                    if (k == ConsoleKey.C) ChristmasColorDemo(nusbioPixel);
                    if (k == ConsoleKey.T) ChristmasTreeDemo(nusbioPixel);

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


