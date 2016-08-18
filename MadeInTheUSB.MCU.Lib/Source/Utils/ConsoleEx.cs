/*
    Copyright (C) 2016 MadeInTheUSB LLC

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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB
{
    /// <summary>
    /// Handle color for the console
    /// </summary>
    public static class ConsoleEx
    {
        [StructLayout(LayoutKind.Sequential)]
        struct POSITION
        {
            public short x;
            public short y;
        }
 
        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int GetStdHandle(int nStdHandle);
 
        [DllImport("kernel32.dll", EntryPoint = "SetConsoleCursorPosition", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetConsoleCursorPosition(int hConsoleOutput, POSITION dwCursorPosition);

        public static void Gotoxy(int x, int y)
        {
            const int STD_OUTPUT_HANDLE = -11;
            int hConsoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            POSITION position;
            position.x = (short) x;
            position.y = (short) y;
            SetConsoleCursorPosition(hConsoleHandle, position);
        }

        public static void WriteMenu(int x, int y, string text)
        {
            if (x == -1)
                x = (GetMaxCol() - text.Length)/2;
            Gotoxy(x, y);
            var i = 0;
            while(i < text.Length)
            {
                if (i < text.Length - 1)
                {
                    if (text[i + 1] == ')')
                    {
                        Write(text[i], ConsoleColor.Cyan);
                        Write(text[i + 1], ConsoleColor.DarkGray);
                        i += 1;
                    }
                    else
                    {
                        Write(text[i], ConsoleColor.DarkCyan);
                    }
                }
                else
                {
                    Write(text[i], ConsoleColor.DarkCyan);
                }
                i += 1;
            }
            Console.WriteLine();
        }

        public static void WriteLine(int x, int y, string text, ConsoleColor c)
        {
            Gotoxy(x, y);
            WriteLine(text, c);
        }

        public static void WriteLine(string text, ConsoleColor c)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = c;
            Console.WriteLine(text);
            Console.ForegroundColor = color;
        }

        public static void Write(int x, int y, string text, ConsoleColor c)
        {
            Gotoxy(x, y);
            Write(text, c);
        }

        static int GetMaxCol()
        {
            return Console.WindowWidth;
        }

        public static int WindowHeight
        {
            get
            {                
                return 25;
            }
        }

        public static void Bar(int x, int y, string text, ConsoleColor textColor, ConsoleColor backGroundColor)
        {
            Gotoxy(0, y);

            Write("".PadLeft(GetMaxCol(), ' '), textColor, backGroundColor);

            Gotoxy(x, y);
            Write(text, textColor, backGroundColor);
        }

        public static void TitleBar(int y, string text, ConsoleColor textColor = ConsoleColor.Yellow, ConsoleColor backGroundColor = ConsoleColor.DarkBlue)
        {
            Gotoxy(0, y);

            Write("".PadLeft(GetMaxCol(), ' '), textColor, backGroundColor);

            Gotoxy((GetMaxCol() - text.Length)/2, y);
            Write(text, textColor, backGroundColor);
        }

        private static void Write(char car, ConsoleColor textColor)
        {
            Write(car.ToString(), textColor);
        }

        public static void Write(int x, int y, string text, ConsoleColor textColor, ConsoleColor? backGroundColor = null)
        {
            if (x == -1)
                x = (GetMaxCol() - text.Length)/2;
            Gotoxy(x, y);
            Write(text, textColor, backGroundColor);
        }

        public static T WaitOnComponentToBePlugged<T>(string componentName, Func<T> initComponentCode)
        {
            while(true) {

                var component = initComponentCode();
                if(component != null)
                    return component;

                var r = ConsoleEx.Question(1, string.Format("Component {0} not found. R)etry A)bandon",componentName), new List<char>() { 'R', 'A'});
                if(r == 'A')
                   return default(T);
            }
        }

        public static char Question(int y, string message, List<char> answers)
        {
            Write(0, y, "".PadLeft(80,' '), ConsoleColor.Yellow, ConsoleColor.Red);
            Write(0, y, message+" ?", ConsoleColor.Yellow, ConsoleColor.Red);
            Gotoxy(message.Length+2, y);
            while (true)
            {
                var k = Console.ReadKey();
                var c = k.KeyChar.ToString().ToUpperInvariant()[0];
                if (answers.Contains(c))
                {
                    return c;
                }
            }
        }

        public static void Write(string text, ConsoleColor textColor, ConsoleColor? backGroundColor = null)
        {
            var bTextColor       = Console.ForegroundColor;
            var bBackGroundColor = Console.BackgroundColor;

            Console.ForegroundColor = textColor;
            if (backGroundColor.HasValue)
                Console.BackgroundColor = backGroundColor.Value;

            Console.Write(text);

            Console.ForegroundColor = bTextColor;
            Console.BackgroundColor = bBackGroundColor;
        }
    }
}
