/*
    Demo application for the NusbioMatrix MCU based.
    Support 1, 4, 8 Matrix.

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
            ConsoleEx.WriteMenu(-1, 2, "L)ight sensor demo  GPIOs Out: 4) 5) 6) 7)  A)nalog Read   D)igital Read");
            ConsoleEx.WriteMenu(-1, 4, "Off) I)nit device  Q)uit");

            var m = string.Format("Firmware {0} v {1}, Port:{2} ", nusbioMatrix.Firmware, nusbioMatrix.FirmwareVersion, nusbioMatrix.ComPort);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 3, m, ConsoleColor.White, ConsoleColor.DarkCyan);
        }

        private static NusbioMCU ConnectToMCU(NusbioMCU nusbioMCU)
        {
            if (nusbioMCU != null)
            {
                nusbioMCU.Close();
                nusbioMCU = null;
            }
            var comPort = new NusbioMCU().DetectMcuComPort();
            if (comPort == null)
            {
                Console.WriteLine("Nusbio MCU not detected");
                return null;
            }
            nusbioMCU = new NusbioMCU(comPort);
            if (nusbioMCU.Initialize().Succeeded)
            {
                NusbioMatrix.AllGpios.ForEach(p => nusbioMCU.SetDigitalPinMode(p, Mcu.DigitalIOMode.OUTPUT));
                return nusbioMCU;
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


        private static void AnalogWritePWMTest(NusbioMCU nusbioMCU)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Analog Write/PWM Demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");
            var quit   = false;
            int speed  = 100;
            int step   = 4;
            var pin    = Mcu.GpioPwmPin.Gpio5;
            var maxPWM = 256 / 1;

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
                Thread.Sleep(speed * 4);
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

        private static void DigitalReadDemo(NusbioMCU nusbioMCU)
        {
            try
            {
                Console.Clear();
                ConsoleEx.TitleBar(0, "Digital Read Demo");
                ConsoleEx.WriteMenu(-1, 6, "Q)uit");
                var quit = false;
                int speed = 0;
                NusbioMatrix.AllGpios.ForEach(p => nusbioMCU.SetDigitalPinMode(p, Mcu.DigitalIOMode.INPUT));

                while (!quit)
                {
                    var adcData = new StringBuilder();
                    adcData.AppendFormat("{0}:", DateTime.Now);
                    foreach (var a in NusbioMatrix.AllGpios)
                    {
                        adcData.AppendFormat("{0}:{1:000} ", a, nusbioMCU.DigitalRead(a));
                    }
                    ConsoleEx.WriteLine(0, 2, adcData.ToString().PadRight(70, ' '), ConsoleColor.Yellow);
                    CheckKeyboard(ref quit, ref speed, nusbioMCU);
                    Thread.Sleep(500);
                }
            }
            finally
            {
                NusbioMatrix.AllGpios.ForEach(p => nusbioMCU.SetDigitalPinMode(p, Mcu.DigitalIOMode.OUTPUT));
            }
        }

        private static string GetAllADCState(NusbioMCU nusbioMCU)
        {
            var adcData = new StringBuilder();
            foreach (var a in NusbioMatrix.AllAdcs)
                adcData.AppendFormat("{0},", nusbioMCU.AnalogRead(a));
            return adcData.ToString();
        }

        private static void TestAnalogPortWithNusbioTestExtension(NusbioMCU nusbioMCU)
        {
            try
            {
                Console.Clear();
                ConsoleEx.TitleBar(0, "Analog Ports Test With Nusbio Test Extension");
                ConsoleEx.WriteMenu(-1, 8, "Q)uit");
                var quit = false;
                int speed = 0;
                var wait = 100;

                nusbioMCU.SetOnBoardLed(NusbioMCU.OnBoardLedMode.On);

                NusbioMatrix.AllGpios.ForEach(p => nusbioMCU.SetDigitalPinMode(p, Mcu.DigitalIOMode.OUTPUT));

                while (!quit)
                {
                    ConsoleEx.WriteLine(0, 2, string.Format("{0}", DateTime.Now), ConsoleColor.Cyan);

                    NusbioMatrix.AllGpios.ForEach(p => nusbioMCU.DigitalWrite(p, false));
                    var testName = "All low ";
                    var adcState = GetAllADCState(nusbioMCU);
                    var passed = adcState == "0,0,0,0,";
                    ConsoleEx.WriteLine(0, 3, string.Format("{0} [{1}] '{2}'", testName, passed ? "PASSED" : "FAILED", adcState), passed ? ConsoleColor.Green : ConsoleColor.Red);

                    if(wait>0) Thread.Sleep(wait);

                    // Turn the 4 gpio high which are connected to the 4 ADC
                    NusbioMatrix.AllGpios.ForEach(p => nusbioMCU.DigitalWrite(p, true));
                    testName = "All high";
                    adcState = GetAllADCState(nusbioMCU);
                    passed   = ValidateADCState(adcState, 1000, 6);
                    ConsoleEx.WriteLine(0, 4, string.Format("{0} Actual:'{1}'", testName, passed ? "PASSED" : "FAILED", adcState), passed ? ConsoleColor.Green : ConsoleColor.Red);

                    NusbioMatrix.AllGpios.ForEach(p => nusbioMCU.DigitalWrite(p, false));

                    // Note that PWM cannot guaranty a value in the ADC it could a 0 or a ~5Volt
                    for (var pwmVal = 0; pwmVal < 255; pwmVal += 10)
                    {
                        Mcu.PwmGpioPins.ForEach(pw => nusbioMCU.AnalogWrite(pw, pwmVal));
                        Thread.Sleep(1);
                        passed   = true;
                        adcState = GetAllADCState(nusbioMCU);
                        ConsoleEx.WriteLine(0, 5, string.Format("{0} pwm:{1} adcState:{2}", 
                            testName, pwmVal, adcState.PadRight(32)), passed ? ConsoleColor.Green : ConsoleColor.Red);
                        Thread.Sleep(wait);
                    }
                    nusbioMCU.AnalogWrite(Mcu.GpioPwmPin.Gpio5, 0);
                    CheckKeyboard(ref quit, ref speed, nusbioMCU);

                    if (wait > 0) Thread.Sleep(wait);
                }
            }
            finally
            {
                NusbioMatrix.AllGpios.ForEach(p => nusbioMCU.DigitalWrite(p, false));
                nusbioMCU.SetOnBoardLed(NusbioMCU.OnBoardLedMode.Connected);
            }
        }

        private static bool ValidateADCState(string csvString, int expectedValue = 1000, int range = 1)
        {
            var vals = csvString.Split(',');
            foreach(var v in vals)
            {
                if (!string.IsNullOrEmpty(v.Trim()))
                {
                    var iv = int.Parse(v);
                    if (!((iv >= expectedValue - range) && (iv <= expectedValue + range)))
                        return false;
                }
            }
            return true;
        }

        static void Main(string[] args)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteLine(0, 0, "", ConsoleColor.Yellow);
            NusbioMCU nusbioMCU = ConnectToMCU(null);
            if (nusbioMCU == null) return;
            var quit = false;
            var halfSecondTimeOut = new TimeOut(500);

            Cls(nusbioMCU);

            while (!quit)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q) quit = true;
                    if (k == ConsoleKey.L) LightSensorDemo(nusbioMCU);

                    if (k == ConsoleKey.D4) nusbioMCU.DigitalWrite(Mcu.GpioPin.Gpio4, !nusbioMCU.DigitalRead(Mcu.GpioPin.Gpio4));
                    if (k == ConsoleKey.D5) nusbioMCU.DigitalWrite(Mcu.GpioPin.Gpio5, !nusbioMCU.DigitalRead(Mcu.GpioPin.Gpio5));
                    if (k == ConsoleKey.D6) nusbioMCU.DigitalWrite(Mcu.GpioPin.Gpio6, !nusbioMCU.DigitalRead(Mcu.GpioPin.Gpio6));
                    if (k == ConsoleKey.D7) nusbioMCU.DigitalWrite(Mcu.GpioPin.Gpio7, !nusbioMCU.DigitalRead(Mcu.GpioPin.Gpio7));
                    if (k == ConsoleKey.O) foreach (var p in NusbioMatrix.AllGpios) nusbioMCU.DigitalWrite(p, false);

                    if (k == ConsoleKey.D) DigitalReadDemo(nusbioMCU);
                    if (k == ConsoleKey.A) AnalogReadDemo(nusbioMCU);
                    if (k == ConsoleKey.W) AnalogWritePWMTest(nusbioMCU);
                    if (k == ConsoleKey.T) TestAnalogPortWithNusbioTestExtension(nusbioMCU);

                    Cls(nusbioMCU);
                }
            }
            nusbioMCU.Close();
        }
    }
}
