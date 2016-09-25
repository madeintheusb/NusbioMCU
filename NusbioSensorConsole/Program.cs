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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadeInTheUSB;
using System.Drawing;
using MadeInTheUSB.MCU;
using MadeInTheUSB.Sensor;

// https://www.youtube.com/watch?v=JgzVCSFaz3I
namespace NusbioMatrixConsole
{
    class Program
    {
        private const int DEFAULT_INTENSITY = 1;
        private const int SAFE_INTENSITY    = 0;

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

        private static void LightSensorDemo(NusbioMCU nusbioMCU)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Light Sensors Demo");
            ConsoleEx.WriteMenu(-1, 5, "Q)uit");

            var quit = false;
            var cycleCounter = 0;
            var speed = 10;

            var lightSensor = AnalogLightSensor.CalibrateLightSensor(new AnalogLightSensor(), AnalogLightSensor.LightSensorType.CdsPhotoCell_3mm_45k_140k);

            while (!quit)
            {
                nusbioMCU.DigitalWrite(Mcu.GpioPin.Gpio4, !nusbioMCU.DigitalRead(Mcu.GpioPin.Gpio4));
                ConsoleEx.WriteLine(0, 1, DateTime.Now.ToString(), ConsoleColor.Yellow);

                lightSensor.SetAnalogValue(nusbioMCU.AnalogRead(Mcu.AdcPin.Adc6));
                ConsoleEx.WriteLine(0, 2, 
                    string.Format("Ligth Sensor: Digital value {0} - {1:0.00} Volt - {2}", lightSensor.AnalogValue, lightSensor.Voltage, lightSensor.CalibratedValue.PadRight(32)), 
                    ConsoleColor.Yellow);

                Thread.Sleep(500);
                CheckKeyboard(ref quit, ref speed, nusbioMCU);
            }
        }

        private static void CheckKeyboard(ref bool quit, ref int speed, NusbioMCU nusbioMCU)
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

        static void Cls(NusbioMCU nusbioMatrix)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(-1, 2, "L)ight sensor demo  GPIOs Out: 4) 5) 6) 7)  A)nalog Read");
            ConsoleEx.WriteMenu(-1, 4, "I)nit device  Q)uit");

            var m = string.Format("Firmware {0} v {1}, Port:{2} ", nusbioMatrix.Firmware, nusbioMatrix.FirmwareVersion, nusbioMatrix.ComPort);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 3, m, ConsoleColor.White, ConsoleColor.DarkCyan);
        }

        private static NusbioMCU ConnectToMCU(NusbioMCU NusbioMCU)
        {
            if (NusbioMCU != null)
            {
                NusbioMCU.Close();
                NusbioMCU = null;
            }
            var comPort = new NusbioMCU().DetectMcuComPort();
            if (comPort == null)
            {
                Console.WriteLine("Nusbio MCU not detected");
                return null;
            }
            NusbioMCU = new NusbioMCU(comPort);
            if (NusbioMCU.Initialize().Succeeded)
            {
                NusbioMCU.SetDigitalPinMode(Mcu.GpioPin.Gpio4, Mcu.DigitalIOMode.OUTPUT);
                NusbioMCU.SetDigitalPinMode(Mcu.GpioPin.Gpio5, Mcu.DigitalIOMode.OUTPUT);
                NusbioMCU.SetDigitalPinMode(Mcu.GpioPin.Gpio6, Mcu.DigitalIOMode.OUTPUT);
                NusbioMCU.SetDigitalPinMode(Mcu.GpioPin.Gpio7, Mcu.DigitalIOMode.OUTPUT);
                return NusbioMCU;
            }
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

        static void Main(string[] args)
        {
            Console.Clear();

            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);

            NusbioMCU nusbioMCU = ConnectToMCU(null);
            if (nusbioMCU == null) return;
            
            var quit = false;

            Cls(nusbioMCU);

            var halfSecondTimeOut = new TimeOut(500);

            while (!quit)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q) quit = true;
                    if (k == ConsoleKey.L)
                        LightSensorDemo(nusbioMCU);

                    if (k == ConsoleKey.D4) nusbioMCU.DigitalWrite(Mcu.GpioPin.Gpio4, !nusbioMCU.DigitalRead(Mcu.GpioPin.Gpio4));
                    if (k == ConsoleKey.D5) nusbioMCU.DigitalWrite(Mcu.GpioPin.Gpio5, !nusbioMCU.DigitalRead(Mcu.GpioPin.Gpio5));
                    if (k == ConsoleKey.D6) nusbioMCU.DigitalWrite(Mcu.GpioPin.Gpio6, !nusbioMCU.DigitalRead(Mcu.GpioPin.Gpio6));
                    if (k == ConsoleKey.D7) nusbioMCU.DigitalWrite(Mcu.GpioPin.Gpio7, !nusbioMCU.DigitalRead(Mcu.GpioPin.Gpio7));
                    if (k == ConsoleKey.A) AnalogReadDemo(nusbioMCU);
                    if (k == ConsoleKey.W) AnalogWritePWMTest(nusbioMCU);

                    Cls(nusbioMCU);
                }
            }
            nusbioMCU.Close();
        }

        private static void AnalogWritePWMTest(NusbioMCU nusbioMCU)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Analog Write/PWM Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");
            var quit = false;
            int speed = 60;
            int step = 4;
            var pin = Mcu.GpioPwmPin.Gpio6;
            var maxPWM = 256 / 2;

            while (!quit)
            {
                for(var i=0; i< maxPWM; i+= step)
                {
                    ConsoleEx.WriteLine(0, 2, string.Format("pin:{0}, value:{1} ", pin, i), ConsoleColor.Yellow);
                    var r = nusbioMCU.AnalogWrite(pin, i);
                    System.Threading.Thread.Sleep(speed);
                    CheckKeyboard(ref quit, ref speed, nusbioMCU);
                    if (quit) break;
                }
                if (quit) break;
                for (var i = maxPWM; i>=0; i -= step)
                {
                    ConsoleEx.WriteLine(0, 2, string.Format("pin:{0}, value:{1} ", pin, i), ConsoleColor.Yellow);
                    var r = nusbioMCU.AnalogWrite(pin, i);
                    System.Threading.Thread.Sleep(speed);
                    CheckKeyboard(ref quit, ref speed, nusbioMCU);
                    if (quit) break;
                }
                nusbioMCU.AnalogWrite(pin, 0);
                Thread.Sleep(speed * 4);
                
            }
        }

        private static void AnalogReadDemo(NusbioMCU nusbioMCU)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Analog Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");
            var quit = false;
            int speed = 0;

            while (!quit)
            {
                //ConsoleEx.WriteLine(0, 2, string.Format(
                //    "A4:{0:00000}, A5:{1:00000}, A6:{2:00000}, A7:{3:00000}, ",
                //    nusbioMCU.AnalogRead(Mcu.AdcPin.Adc4),
                //    nusbioMCU.AnalogRead(Mcu.AdcPin.Adc5),
                //    nusbioMCU.AnalogRead(Mcu.AdcPin.Adc6),
                //    nusbioMCU.AnalogRead(Mcu.AdcPin.Adc7)
                //    ), ConsoleColor.Yellow);

                var adcData = new StringBuilder();
                foreach (var a in NusbioMatrix.AllAdcs)
                {
                    adcData.AppendFormat("{0}:{1:000} ", a, nusbioMCU.AnalogRead(a));
                }
                ConsoleEx.WriteLine(0, 2, adcData.ToString(), ConsoleColor.Yellow);
                CheckKeyboard(ref quit, ref speed, nusbioMCU);
                Thread.Sleep(500);
            }
        }
    }
}
