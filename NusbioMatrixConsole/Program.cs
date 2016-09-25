/*
    Demo application for the NusbioMCU and 1 or 4 8x8 LED Matrix.
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadeInTheUSB;
using System.Drawing;
using MadeInTheUSB.MCU;

// https://www.youtube.com/watch?v=JgzVCSFaz3I
namespace NusbioMatrixConsole
{
    class Program
    {
        private const int DEFAULT_INTENSITY = 2;
        private const int SAFE_INTENSITY    = 1;

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

        class RectangleA
        {
            public Rectangle Rect;
            public bool On;

            public RectangleA(int x, int y, int w, int h, bool on)
            {
                this.Rect = new Rectangle(x, y, w, h);
                this.On = on;
            }
        }

        class CircleA
        {
            public Point Point;
            public int Ray;
            public bool On;

            public CircleA(int x, int y, int r, bool on)
            {
                this.Point = new Point(x, y);
                this.Ray = r;
                this.On = on;
            }

            public NusbioMatrix Draw(NusbioMatrix nusbioMatrix)
            {
                nusbioMatrix.Circle(this.Point.X, this.Point.Y, this.Ray, this.On).WriteDisplay();
                return nusbioMatrix;
            }
        }

        static void DotDemo(NusbioMatrix nusbioMatrix)
        {
            var quit = false;
            var intensity = SAFE_INTENSITY;
            var speed = nusbioMatrix.MatrixCount == 8 ? 1 : nusbioMatrix.MatrixCount == 1 ? 10 : 2;
            nusbioMatrix.SetIntensity(intensity);

            while (!quit)
            {
                Console.Clear();
                ConsoleEx.TitleBar(0, "Dot Demo");
                ConsoleEx.WriteMenu(-1, 5, "Q)uit -) Faster +) Slower");
                nusbioMatrix.Clear(true);
                nusbioMatrix.SetIntensity(intensity);

                var y = 0;
                while (y < nusbioMatrix.Height)
                {
                    var x = 0;
                    while (x < nusbioMatrix.Width)
                    {
                        ConsoleEx.Write(0, 2, string.Format("x:{0:00}, y:{1:00}, intensity:{2:00}, speed:{3:00}  ", x, y, intensity, speed), ConsoleColor.White);
                        ConsoleEx.Write(x + 1, y + 7, ".", ConsoleColor.White);
                        nusbioMatrix.SetLed(x, y, true).WriteRow(y);

                        if (speed > 0) nusbioMatrix.Sleep(speed);

                        CheckKeyboard(ref quit, ref speed, nusbioMatrix);

                        if (quit) break;
                        x++;
                    }
                    if (quit) break;
                    y++;
                }

                ConsoleEx.WriteMenu(0, 3, nusbioMatrix.GetByteSecondSentStatus(true));

                // ~~~ Increasing intensisty will create a current consumption outage 
                // ~~~ depending on ATMega328 break. See comment on MAX7219_MAX_INTENSITY
                // intensity += 1; // << May cause issue
                if (intensity > NusbioMatrix.MAX7219_MAX_INTENSITY)
                    intensity = 0;

                var waitScreen = 115;
                if (nusbioMatrix.MatrixCount == 4)
                {
                    nusbioMatrix.Clear(true, 1).Sleep(waitScreen)
                        .Clear(true, 2).Sleep(waitScreen)
                        .Clear(true, 0).Sleep(waitScreen)
                        .Clear(true, 3).Sleep(waitScreen);
                }
                else if (nusbioMatrix.MatrixCount == 8)
                {
                    nusbioMatrix.Clear(true, 3).Sleep(waitScreen)
                        .Clear(true, 4).Sleep(waitScreen)
                        .Clear(true, 2).Sleep(waitScreen)
                        .Clear(true, 5).Sleep(waitScreen)
                        .Clear(true, 1).Sleep(waitScreen)
                        .Clear(true, 6).Sleep(waitScreen)
                        .Clear(true, 0).Sleep(waitScreen)
                        .Clear(true, 7).Sleep(waitScreen);
                }
            }
            nusbioMatrix.Clear(true);
            nusbioMatrix.SetIntensity(DEFAULT_INTENSITY);
        }

        static void RectangleDemo(NusbioMatrix nusbioMatrix)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Rectangle Demo");
            ConsoleEx.WriteMenu(-1, 5, "Q)uit -) Faster +) Slower");
            nusbioMatrix.Clear(true);
            //nusbioMatrix.TestSimulator();

            var quit = false;
            var speed = 200;
            var maxX = nusbioMatrix.MatrixCount*NusbioMatrix.MATRIX_COL_SIZE;
            var maxY = NusbioMatrix.MATRIX_ROW_SIZE;
            var x = 0;
            var y = 0;
            var animation = new List<RectangleA>();
            var animation2 = new List<RectangleA>();

            for (var i = 0; i < 3; i ++)
            {
                animation.Add(new RectangleA(x, y, maxX, maxY, true));
                animation2.Add(new RectangleA(x, y, maxX, maxY, false));
                x += 2;
                y += 2;
                maxX -= 4;
                maxY -= 4;
            }
            animation2.Reverse();
            animation.AddRange(animation2);

            while (!quit)
            {
                foreach (var a in animation)
                {
                    ConsoleEx.Write(0, 2, string.Format("x:{0:00}, y:{1:00}, w:{2:00}, h:{3:00}, Speed:{4:000}", a.Rect.X, a.Rect.Y, a.Rect.Width, a.Rect.Height, speed), ConsoleColor.White);
                    nusbioMatrix.Rectangle(a.Rect, a.On).WriteDisplay();

                    if (speed > 0) nusbioMatrix.Sleep(speed);
                    CheckKeyboard(ref quit, ref speed, nusbioMatrix);
                    if (quit) break;

                    ///nusbioMatrix.Recorder.Save(@"c:\seq.json");
                }
                ConsoleEx.WriteMenu(0, 3, nusbioMatrix.GetByteSecondSentStatus(true));
            }
        }

        static void CircleDemo(NusbioMatrix nusbioMatrix)
        {
            var quit = false;
            var speed = nusbioMatrix.MatrixCount == 1 ? 80 : 40;
            speed = 5;
            var animations = new List<CircleA>();

            for (var d = 0; d < nusbioMatrix.MatrixCount; d++)
            {
                var x = 3 + (d*NusbioMatrix.MATRIX_COL_SIZE);
                animations.Add(new CircleA(x, 3, 2, true));
                animations.Add(new CircleA(x, 3, 3, true));
                animations.Add(new CircleA(x, 3, 0, true)); // Draw nothing
                animations.Add(new CircleA(x, 3, 0, true)); // Draw nothing
                animations.Add(new CircleA(x, 3, 3, false));
                animations.Add(new CircleA(x, 3, 2, false));
            }

            Console.Clear();
            ConsoleEx.TitleBar(0, "Circle Demo");
            ConsoleEx.WriteMenu(-1, 5, "Q)uit  -) Faster +) Slower");
            nusbioMatrix.Clear(true);

            while (!quit)
            {
                nusbioMatrix.ResetBytePerSecondCounters();
                foreach (var animation in animations)
                {
                    ConsoleEx.Write(0, 2, string.Format("x:{0:00}, y:{1:00}, r:{2:00}, speed:{3:000}", animation.Point.X, animation.Point.Y, animation.Ray, speed), ConsoleColor.White);
                    animation.Draw(nusbioMatrix).WriteDisplay();

                    if (speed > 0) nusbioMatrix.Sleep(speed);

                    CheckKeyboard(ref quit, ref speed, nusbioMatrix);
                    if (quit) break;
                }
                ConsoleEx.WriteMenu(0, 3, nusbioMatrix.GetByteSecondSentStatus(true));
            }
        }

        static void TextDemo(NusbioMatrix nusbioMatrix)
        {
            //var text = "Hello World! " +
            var text = "NusbioMatrix for .NET rocks..." +
                       (nusbioMatrix.MatrixCount == 4
                           ? "  "
                           : (nusbioMatrix.MatrixCount == 8 ? "     " : ""));
            var speed = nusbioMatrix.MatrixCount == 1 ? 100 : 30;
            var quit = false;

            Console.Clear();
            ConsoleEx.TitleBar(0, "Text Demo");
            ConsoleEx.WriteMenu(-1, 5, "Q)uit  -) Faster +) Slower");

            nusbioMatrix.Clear(true);
            nusbioMatrix.SetIntensity(3);

            while (!quit)
            {
                for (var charIndex = 0; charIndex < text.Length; charIndex++)
                {
                    var c = text[charIndex];
                    ConsoleEx.WriteMenu(0, 2, string.Format("Speed:{0:00}, Text:[{1}]", speed, text.Substring(0, charIndex + 1)).PadRight(64));
                    for (var rr = 0; rr < NusbioMatrix.MATRIX_ROW_SIZE; rr++)
                    {
                        nusbioMatrix.ScrollPixelLeftDevices(1, 1)
                            .WriteCharColumn(nusbioMatrix.MatrixCount - 1, c, rr, NusbioMatrix.MATRIX_ROW_SIZE - 1)
                            .WriteDisplay();

                        if (speed > 0) Thread.Sleep(speed);

                        //nusbioMatrix.WriteDisplay();
                        //nusbioMatrix.DrawFrame(0);

                        CheckKeyboard(ref quit, ref speed, nusbioMatrix);
                        if(quit) break;
                    }
                }
                ConsoleEx.WriteMenu(0, 3, nusbioMatrix.GetByteSecondSentStatus(true));
                nusbioMatrix.SetIntensity(DEFAULT_INTENSITY);
            }
        }



        private static void LandscapeDemo(NusbioMatrix nusbioMatrix, int deviceIndex = 0)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Random Landscape Demo");
            ConsoleEx.WriteMenu(-1, 5, "Q)uit  -) Faster +) Slower");

            var landscape = new NusbioLandscapeMatrix(nusbioMatrix, 0);
            var speed     = nusbioMatrix.MatrixCount == 1 ? 150 : 110;

            nusbioMatrix.Clear(true);
            var quit = false;
            var cycleCounter = 0;

            while (!quit)
            {
                
                landscape.Redraw();
                ConsoleEx.WriteLine(0, 2, string.Format("{0}, speed:{1:000}", landscape, speed), ConsoleColor.Cyan);

                if (speed > 0) Thread.Sleep(speed);

                CheckKeyboard(ref quit, ref speed, nusbioMatrix);

                cycleCounter += 1;
                if (cycleCounter >= nusbioMatrix.Width)
                {
                    ConsoleEx.WriteMenu(0, 3, nusbioMatrix.GetByteSecondSentStatus(true));
                    cycleCounter = 0;
                }
            }
        }

        private static void CheckKeyboard(ref bool quit, ref int speed, NusbioMatrix nusbioMatrix)
        {
            /*
            var adcData = new StringBuilder();
            foreach (var a in NusbioMatrix.AllAdcs)
            {
                adcData.AppendFormat("{0}:{1:000} ", a, nusbioMatrix.AnalogRead(a));
            }
            ConsoleEx.Bar(0, 23, adcData.ToString(), ConsoleColor.Yellow, ConsoleColor.DarkGray);
            
            foreach (var g in NusbioMatrix.AllGpios)
            {
                nusbioMatrix.DigitalWrite(g, !nusbioMatrix.DigitalRead(g));
            }
            */

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

        static void Cls(NusbioMatrix nusbioMatrix)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(-1, 2, "R)ectangle demo   C)ircle demo   D)ot demo  T)ext demo  L)andscape demo");
            ConsoleEx.WriteMenu(-1, 4, "I)nit device  Q)uit");

            var maxtrixCount = nusbioMatrix.GetMatrixCount();
            var m = string.Format("Firmware {0} v {1}, Port:{2}, Maxtrix Count:{3}, Intensity:{4}", nusbioMatrix.Firmware, nusbioMatrix.FirmwareVersion, nusbioMatrix.ComPort, maxtrixCount, nusbioMatrix.Intensity);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 3, m, ConsoleColor.White, ConsoleColor.DarkCyan);
            nusbioMatrix.Clear(true);
        }

        private static NusbioMatrix ConnectToMCU(int matrixCount, NusbioMatrix nusbioMatrix)
        {
            if (nusbioMatrix != null)
            {
                nusbioMatrix.Dispose();
                nusbioMatrix = null;
            }
            var comPort = new NusbioMatrix().DetectMcuComPort();
            if (comPort == null)
            {
                Console.WriteLine("Nusbio Matrix not detected");
                return null;
            }
            nusbioMatrix = new NusbioMatrix(comPort, matrixCount);
            if (nusbioMatrix.Initialize().Succeeded)
            {
                nusbioMatrix.SetIntensity(DEFAULT_INTENSITY);
                return nusbioMatrix;
            }
            return null;
        }

        static void Main(string[] args)
        {
            Console.Clear();

            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            var matrixCountChar = ConsoleEx.Question(1, "Maxtrix Count 1, 2, 4 or 8", new List<char>() {'1', '2', '4', '8'});
            //var matrixCountChar = '4';
            var matrixCount = (int) matrixCountChar - 48;
            Console.WriteLine("");

            NusbioMatrix nusbioMatrix = ConnectToMCU(matrixCount, null);
            if (nusbioMatrix == null) return;
            
            Cls(nusbioMatrix);

            var quit = false;
            while (!quit)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;

                    if (k == ConsoleKey.Q) quit = true;
                    if (k == ConsoleKey.R) RectangleDemo(nusbioMatrix);
                    if (k == ConsoleKey.D) DotDemo(nusbioMatrix);
                    if (k == ConsoleKey.C) CircleDemo(nusbioMatrix);
                    if (k == ConsoleKey.T) TextDemo(nusbioMatrix);
                    if (k == ConsoleKey.L) LandscapeDemo(nusbioMatrix);
                    if (k == ConsoleKey.I) nusbioMatrix = ConnectToMCU(matrixCount, nusbioMatrix);

                    Cls(nusbioMatrix);
                }
            }
            nusbioMatrix.Dispose();
        }
    }
}
