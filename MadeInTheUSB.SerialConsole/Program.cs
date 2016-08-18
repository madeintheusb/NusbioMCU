using System;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using ArduinoLibrary;
using ArduinoWindowsConsole;
using DynamicSugar;
using System.Runtime.InteropServices;

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

        static void SendCommand(ArduinoConnection ac , string command)
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

        static void Main(string[] args)
        {
            var goOn         = true;
            var processQueue = true;

            InitConfig();

            PrintHelp();

            bool displayPaused = false;

            // D:\DVT\Arduino\arduino-1.6.5\libraries\MPU6050


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
                            case ComCommandType.Send                     :
                                SendCommand(ac, comCommand.Command);                                               break;
                            case ComCommandType.Help                     : Console.Clear(); PrintHelp();                                   break;
                            case ComCommandType.Quit                     : goOn = false;                                                                      break;
                            case ComCommandType.PauseProcessingFromDevice:
                                displayPaused = !displayPaused;
                                //processQueue = !processQueue; Console.WriteLine("ProcessQueue:{0}", processQueue);
                                break;
                        }
                    }
                    if (processQueue && ac.ReceivedMessages.Count > 0)
                    {
                        var message = ac.ReceivedMessages.Dequeue();
                        if(!displayPaused)
                            WriteLine(message, message.StartsWith("<") ? ConsoleColor.Green : ConsoleColor.DarkGreen);
                        //if (message.Contains("<MotionDetected>"))
                        //    LockWorkStation();
                    }
                }
            }
        }
    }
 }