using System;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using ArduinoLibrary;
using ArduinoWindowsConsole;
using DynamicSugar;
using System.Runtime.InteropServices;
using STDDeviation;

namespace test
{
    class Program
    {
        private static ComConfig _comConfig;

        static void WriteLine(string text, ConsoleColor c)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = c;
            Console.WriteLine(text);
            Console.ForegroundColor = color;
        }

        static void SendCommand(ArduinoConnection ac, string command)
        {
            WriteLine(command, ConsoleColor.Cyan);
            ac.Send(command);
        }

        private static void PrintHelp()
        {
            WriteLine("Communication to {0} - (port:{1})".FormatString(_comConfig.DeviceName, _comConfig.PortName), ConsoleColor.Cyan);
            WriteLine(_comConfig.GetMenu(), ConsoleColor.Cyan);
            Console.Title = string.Format("Serial Console - {0}:{1} - {2}", _comConfig.DeviceName, _comConfig.PortName, _comConfig.GetMenu());
        }

        static string GetConfigFileName()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ArduinoWindowsConsole.json");
        }

        static void InitConfig()
        {
            _comConfig = ComConfig.Load(GetConfigFileName());
        }

        static void WriteToFile(string text)
        {
            System.IO.File.AppendAllText(@"c:\acc_gyro.txt", text);
        }

        static void Main(string[] args)
        {
            var goOn = true;
            var processQueue = true;

            InitConfig();
            PrintHelp();

            bool displayPaused = false;

            // D:\DVT\Arduino\arduino-1.6.5\libraries\MPU6050

            var b = new System.Text.StringBuilder(1000);

            var bikeMotions = new BikeMotions();

            using (var ac = new ArduinoConnection(_comConfig.PortName, _comConfig.BaudRate))
            {
                Console.WriteLine("Port Open");
                while (goOn)
                {
                    if (Console.KeyAvailable)
                    {
                        var k = Console.ReadKey(true).Key;
                        var comCommand = _comConfig.GetCommand(k);
                        switch (comCommand.CommandType)
                        {
                            case ComCommandType.Send:
                                SendCommand(ac, comCommand.Command); break;
                            case ComCommandType.Help: Console.Clear(); PrintHelp(); break;
                            case ComCommandType.Quit: goOn = false; break;
                            case ComCommandType.PauseProcessingFromDevice:
                                displayPaused = !displayPaused;
                                //processQueue = !processQueue; Console.WriteLine("ProcessQueue:{0}", processQueue);
                                break;
                        }
                    }
                    if (processQueue && ac.ReceivedMessages.Count > 0)
                    {
                        const string mark = "mean:";

                        var message = ac.ReceivedMessages.Dequeue();

                        if(message.Contains(mark))
                            Console.WriteLine("mean {0}",message);

                        var processRow = !message.Contains(mark);

                        if(string.IsNullOrEmpty(message))
                           processRow = false; 

                        if ((!displayPaused) && (processRow))
                        {
                            BikeMotion newBikeMotion = null;
                            //WriteLine(message, message.StartsWith("<") ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                            var sss = message;
                            sss = sss.Replace("\r", "");
                            var p = sss.Split(',');

                            if(p.Length == 6) {
                                newBikeMotion = bikeMotions.Add(p[0], p[1], p[2], p[3], p[4], p[5]);
                                Console.WriteLine("\r\nRaw {0}",sss);
                            }
                            else
                            {
                                Console.WriteLine("? {0}",message);
                            }

                            if(newBikeMotion != null)
                            {
                                if (bikeMotions.Count >= BikeMotions.MAX_SAMPLE_PER_SECOND)
                                {
                                    bikeMotions.CalcStdDeviationEverySecondXY();
                                    // Compute base on the 2 last seconds
                                    if(bikeMotions.Count >= BikeMotions.MAX_SAMPLE_PER_SECOND) {
                                        bikeMotions.RemoveAt(0);
                                    }
                                }
                                //b.AppendFormat("{0},", sss).AppendLine();
                                if (b.Length > 4096)
                                {
                                    ///WriteToFile(b.ToString());
                                    b.Clear();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}