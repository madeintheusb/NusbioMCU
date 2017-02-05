'#Const _300_LEDS = True
'
'    Demo application for the NusbioMCU and Multi-Color LED (RGB, WS2812)
'    Copyright (C) 2016 MadeInTheUSB LLC
'    Written by FT
'    
'    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
'    associated documentation files (the "Software"), to deal in the Software without restriction, 
'    including without limitation the rights to use, copy, modify, merge, publish, distribute, 
'    sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
'    furnished to do so, subject to the following conditions:
'
'    The above copyright notice and this permission notice shall be included in all copies or substantial 
'    portions of the Software.
'
'    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
'    LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
'    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
'    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
'    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
'

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports MadeInTheUSB
Imports System.Drawing
Imports MadeInTheUSB.MCU
Imports MadeInTheUSB.Components

Namespace NusbioPixelConsole

    Class Program
        Shared _rgbLedType As NusbioPixelDeviceType

        Public Shared Function GetAssemblyCopyright() As String
            Dim currentAssem As Assembly = GetType(Program).Assembly
            Dim attribs As Object() = currentAssem.GetCustomAttributes(GetType(AssemblyCopyrightAttribute), True)
            If attribs.Length > 0 Then
                Return DirectCast(attribs(0), AssemblyCopyrightAttribute).Copyright
            End If
            Return Nothing
        End Function

        Private Shared Function GetAssemblyProduct() As String
            Dim currentAssem As Assembly = GetType(Program).Assembly
            Dim attribs As Object() = currentAssem.GetCustomAttributes(GetType(AssemblyProductAttribute), True)
            If attribs.Length > 0 Then
                Return DirectCast(attribs(0), AssemblyProductAttribute).Product
            End If
            Return Nothing
        End Function

        Private Shared Function CheckKeyboard(ByRef quit As Boolean, ByRef speed As Integer) As Boolean
            If Console.KeyAvailable Then
                Select Case Console.ReadKey(True).Key
                    Case ConsoleKey.Q
                        quit = True
                        Exit Select
                    Case ConsoleKey.Add, ConsoleKey.OemPlus
                        speed += 10
                        Exit Select
                    Case ConsoleKey.Subtract, ConsoleKey.OemMinus
                        speed -= 10
                        If speed < 0 Then
                            speed = 0
                        End If
                        Exit Select
                End Select
            End If
            Return quit
        End Function

        Private Shared Sub Cls(nusbioMatrix As NusbioPixel)
            Console.Clear()
            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue)

            ConsoleEx.WriteMenu(-1, 2, "0) Rainbow all strip demo  1) Rainbow spread demo  S)quare demo  L)ine demo")
            ConsoleEx.WriteMenu(-1, 6, "I)nit device  Q)uit")

            'var maxtrixCount = nusbioMatrix.Count;
            Dim m = String.Format("Firmware {0} v {1}, Port:{2}, LED Count:{3}", nusbioMatrix.Firmware, nusbioMatrix.FirmwareVersion, nusbioMatrix.ComPort, nusbioMatrix.Count)
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 3, m, ConsoleColor.White, ConsoleColor.DarkCyan)
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue)
        End Sub

        Private Shared Function ConnectToMCU(nusbioPixel As NusbioPixel, maxLed As Integer) As NusbioPixel
            If nusbioPixel IsNot Nothing Then
                nusbioPixel.Dispose()
                nusbioPixel = Nothing
            End If
            Dim comPort = New NusbioPixel().DetectMcuComPort()
            If comPort Is Nothing Then
                Console.WriteLine("Nusbio Pixel not detected")
                Return Nothing
            End If
            nusbioPixel = New NusbioPixel(maxLed, comPort)
            If nusbioPixel.Initialize().Succeeded Then
                If nusbioPixel.SetBrightness(nusbioPixel.DEFAULT_BRIGHTNESS).Succeeded Then
                    If nusbioPixel.Firmware = Mcu.FirmwareName.NusbioMcu2StripPixels Then
                        nusbioPixel.PowerMode = PowerMode.EXTERNAL
                        If nusbioPixel.SetBrightness(nusbioPixel.DEFAULT_BRIGHTNESS, NusbioPixel.StripIndex.S1).Succeeded Then
                            Return nusbioPixel
                        End If
                    Else
                        Return nusbioPixel
                    End If
                End If
            End If
            Return Nothing
        End Function

        Private Shared Function ToHexValue(color As Color) As String
            Return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2")
        End Function

        Private Enum RainbowEffect
            AllStrip
            Spread
        End Enum


        Private Shared Sub SquareDemo(nusbioPixel As NusbioPixel)

            Console.Clear()
            ConsoleEx.TitleBar(0, "Square 8x8 Demo")
            ConsoleEx.WriteMenu(-1, 6, "Q)uit")

            Dim speed As Integer = If(nusbioPixel.Count <= 16, 32, 16)
            Dim quit = False
            Dim jStep = 32
            Dim bkColor As Color
            Dim i As Int16

            While Not quit
                Dim j = 0
                While j < 256
                    ConsoleEx.WriteLine(0, 2, String.Format("jStep:{0:000}", j), ConsoleColor.White)

                    For i = 0 To nusbioPixel.Count - 1
                        If i = 0 OrElse True Then
                            bkColor = RGBHelper.Wheel((i + j) And 255)
                            nusbioPixel.SetStrip(bkColor).Show()
                        End If

                        Dim newColor = Color.FromArgb(bkColor.B, bkColor.G, bkColor.R)
                        nusbioPixel.SetPixel(i, newColor)
                        nusbioPixel.Show()
                        If speed > 0 Then
                            Thread.Sleep(speed)
                        End If
                        CheckKeyboard(quit, speed)
                        If quit Then
                            Exit For
                        End If
                    Next
                    If speed > 0 Then
                        Thread.Sleep(speed * 2)
                    End If
                    If quit Then
                        Exit While
                    End If
                    j += jStep
                End While
                ConsoleEx.Write(0, 24, nusbioPixel.GetByteSecondSentStatus(True), ConsoleColor.Cyan)
            End While
        End Sub

        Private Shared Sub LineDemo(nusbioPixel As NusbioPixel)

            Console.Clear()
            ConsoleEx.TitleBar(0, "Line Demo")
            ConsoleEx.WriteMenu(-1, 6, "Q)uit")

            Dim maxShowPerformance As Long = 0
            Dim minShowPerformance As Long = Long.MaxValue

            nusbioPixel.SetBrightness(nusbioPixel.DEFAULT_BRIGHTNESS)

            Dim quit = False
            Dim speed As Integer = If(nusbioPixel.Count < 30, 75, 0)
            Dim wheelColorMaxStep = 256
            Dim jWheelColorStep = 4
            Dim color1 As Color = Color.Beige
            Dim i As Int16

            While Not quit
                Dim jWheelColorIndex = 0
                While jWheelColorIndex < wheelColorMaxStep
                    ' Set the background color of the strip
                    ConsoleEx.WriteLine(0, 2, String.Format("jWheelColorIndex:{0:000}, jWheelColorStep:{1:000}, Speed:{2:000}", jWheelColorIndex, jWheelColorStep, speed), ConsoleColor.White)
                    Dim sw = Stopwatch.StartNew()
                    color1 = RGBHelper.Wheel((jWheelColorIndex) And 255)
                    For i = 0 To nusbioPixel.Count - 1
                        ' Set all te pixel to one color
                        If i = 0 Then
                            nusbioPixel.SetPixel(i, color1)
                        Else
                            ' Set led index to 0
                            nusbioPixel.SetPixel(color1)
                        End If

                        If i Mod 4 = 0 Then
                            Console.WriteLine()
                        End If
                        ' , ToHexValue(color) html value
                        Console.Write("[{0:x2}]rgb:{1:x2},{2:x2},{3:x2} ", i, color1.R, color1.G, color1.B)
                    Next
                    sw.[Stop]()
                    ConsoleEx.Write(0, 20, String.Format("SetPixel() Time:{0:000}ms, {1}", sw.ElapsedMilliseconds, nusbioPixel.GetByteSecondSentStatus(True)), ConsoleColor.Cyan)
                    sw = Stopwatch.StartNew()
                    nusbioPixel.Show()
                    sw.[Stop]()
                    If sw.ElapsedMilliseconds < minShowPerformance Then
                        minShowPerformance = sw.ElapsedMilliseconds
                    End If
                    If sw.ElapsedMilliseconds > maxShowPerformance Then
                        maxShowPerformance = sw.ElapsedMilliseconds
                    End If
                    ConsoleEx.Write(0, 21, String.Format("Show() Time:{0:000}ms max:{1:000} min:{2:000}, {3}", sw.ElapsedMilliseconds, maxShowPerformance, minShowPerformance, nusbioPixel.GetByteSecondSentStatus(True)), ConsoleColor.Cyan)

                    Dim halfLedCount = (nusbioPixel.Count / 2) + 1
                    ' Set the foreground Color or animation color scrolling
                    sw = Stopwatch.StartNew()
                    Dim fgColor = RGBHelper.Wheel((jWheelColorIndex + CInt(jWheelColorStep * 4)) And 255)
                    For i = 0 To halfLedCount - 1
                        nusbioPixel.SetPixel(i, fgColor)
                        nusbioPixel.SetPixel(nusbioPixel.Count - i, fgColor)
                        nusbioPixel.Show()
                        If speed > 0 Then
                            Thread.Sleep(speed)
                        End If
                        ' Give a better effect
                        If CheckKeyboard(quit, speed) Then
                            Exit For
                        End If
                    Next
                    sw.[Stop]()
                    ConsoleEx.Write(0, 22, String.Format("show() {0} times for animation time:{1:000}ms {2:000}, {3}", halfLedCount, sw.ElapsedMilliseconds, sw.ElapsedMilliseconds / halfLedCount, nusbioPixel.GetByteSecondSentStatus(True)), ConsoleColor.Cyan)

                    If speed > 0 Then
                        Thread.Sleep(speed)
                    End If
                    If CheckKeyboard(quit, speed) OrElse quit Then
                        Exit While
                    End If
                    jWheelColorIndex += jWheelColorStep
                End While
            End While
        End Sub

        Public Shared Function GetBlendedColor(percentage As Integer) As Color
            If percentage < 50 Then
                Return Interpolate(Color.Red, Color.Yellow, percentage / 50.0)
            End If
            Return Interpolate(Color.Yellow, Color.Green, (percentage - 50) / 50.0)
        End Function

        Private Shared Function Interpolate(color1 As Color, color2 As Color, fraction As Double) As Color
            Dim r As Double = Interpolate(color1.R, color2.R, fraction)
            Dim g As Double = Interpolate(color1.G, color2.G, fraction)
            Dim b As Double = Interpolate(color1.B, color2.B, fraction)
            Return Color.FromArgb(CInt(Math.Round(r)), CInt(Math.Round(g)), CInt(Math.Round(b)))
        End Function

        Private Shared Function Interpolate(d1 As Double, d2 As Double, fraction As Double) As Double
            'return d1 + (d1 - d2) * fraction;
            Return d1 + (d2 - d1) * fraction
        End Function

        Private Shared Sub RainbowDemo(nusbioPixel As NusbioPixel, rainbowEffect__2 As RainbowEffect)
            Console.Clear()
            ConsoleEx.TitleBar(0, "Rainbow Demo")
            ConsoleEx.WriteMenu(-1, 6, "Q)uit")

            Dim maxShowPerformance As Long = 0
            Dim minShowPerformance As Long = Long.MaxValue
            Dim quit = False
            Dim speed As Integer = 10
            Dim jWheelColorStep = 4

            Dim brightness = 190
            ' This is for NusbioMCUpixel, will automatically reduce for NusbioMCU
            nusbioPixel.SetBrightness(brightness)
            If nusbioPixel.Firmware = Mcu.FirmwareName.NusbioMcu2StripPixels Then
                nusbioPixel.SetBrightness(brightness, NusbioPixel.StripIndex.S1)
            End If

            If nusbioPixel.Firmware = Mcu.FirmwareName.NusbioMcu2StripPixels Then
                speed /= 2
            End If

            If nusbioPixel.Count > 100 Then
                speed = 0
            End If

            While Not quit
                Dim jWheelColorIndex = 0
                While jWheelColorIndex < 256
                    ConsoleEx.WriteLine(0, 2, String.Format("jWheelColorIndex:{0:000}, jWheelColorStep:{1:00}", jWheelColorIndex, jWheelColorStep), ConsoleColor.White)
                    Dim sw = Stopwatch.StartNew()

                    For iStrip0 As Integer = 0 To nusbioPixel.Count - 1
                        Dim color3 = Color.Beige

                        If rainbowEffect__2 = RainbowEffect.AllStrip Then
                            color3 = RGBHelper.Wheel((jWheelColorIndex) And 255)
                        ElseIf rainbowEffect__2 = RainbowEffect.Spread Then
                            color3 = RGBHelper.Wheel((iStrip0 * 256 / nusbioPixel.Count) + jWheelColorIndex)
                        End If

                        If iStrip0 = 0 Then
                            ' Setting the pixel this way, will support more than 255 LED
                            nusbioPixel.SetPixel(iStrip0, color3.R, color3.G, color3.B)
                        Else
                            ' Set led index to 0
                            nusbioPixel.SetPixel(color3.R, color3.G, color3.B)
                        End If
                        ' Set led index to 0
                        If nusbioPixel.Firmware = Mcu.FirmwareName.NusbioMcu2StripPixels AndAlso nusbioPixel.Count <= 120 Then
                            If iStrip0 = 0 Then
                                ' Setting the pixel this way, will support more than 255 LED
                                nusbioPixel.SetPixel(iStrip0, color3.R, color3.G, color3.B, NusbioPixel.StripIndex.S1)
                            Else
                                ' Set led index to 0
                                nusbioPixel.SetPixel(color3.R, color3.G, color3.B, NusbioPixel.StripIndex.S1)
                            End If
                        End If

                        If nusbioPixel.Count <= 120 Then
                            If iStrip0 Mod 4 = 0 Then
                                Console.WriteLine()
                            End If
                            ' , ToHexValue(color) html value
                            Console.Write("[{0:x2}]rgb:{1:x2},{2:x2},{3:x2} ", iStrip0, color3.R, color3.G, color3.B)
                        End If
                    Next
                    'sw.Stop();
                    'ConsoleEx.Write(0, 22, string.Format("SetPixel() Time:{0:000}ms, {1}", sw.ElapsedMilliseconds, nusbioPixel.GetByteSecondSentStatus(true)), ConsoleColor.Cyan);
                    'sw = Stopwatch.StartNew();
                    nusbioPixel.Show()
                    If nusbioPixel.Firmware = Mcu.FirmwareName.NusbioMcu2StripPixels Then
                        nusbioPixel.Show(NusbioPixel.StripIndex.S1)
                    End If
                    sw.[Stop]()

                    If sw.ElapsedMilliseconds < minShowPerformance Then
                        minShowPerformance = sw.ElapsedMilliseconds
                    End If
                    If sw.ElapsedMilliseconds > maxShowPerformance Then
                        maxShowPerformance = sw.ElapsedMilliseconds
                    End If

                    ConsoleEx.Write(0, 23, String.Format("SetPixel()/Show() {0}", nusbioPixel.GetByteSecondSentStatus(True)), ConsoleColor.Cyan)

                    If speed > 0 Then
                        Thread.Sleep(speed)
                    End If
                    CheckKeyboard(quit, speed)
                    If quit Then
                        Exit While
                    End If
                    jWheelColorIndex += jWheelColorStep

                End While
            End While
        End Sub

        Private Shared Sub RingDemo(nusbioPixel As NusbioPixel, Optional deviceIndex As Integer = 0)
            Console.Clear()
            ConsoleEx.TitleBar(0, "Rainbow Demo")
            ConsoleEx.WriteMenu(-1, 6, "Q)uit")
            Dim speed As Integer = 0
            Dim quit = False
            Dim jStep = 4

            nusbioPixel.SetBrightness(24)

            While Not quit
                For jStep = 2 To 23 Step 2
                    ConsoleEx.WriteLine(0, 2, String.Format("jStep:{0:000}", jStep), ConsoleColor.White)

                    Dim j = 0
                    While j < 256
                        For i As Integer = 0 To nusbioPixel.Count - 1
                            Dim color = RGBHelper.Wheel((i * 256 / nusbioPixel.Count) + j)
                            If i = 0 Then
                                nusbioPixel.SetPixel(i, color.R, color.G, color.B)
                            Else
                                nusbioPixel.SetPixel(color.R, color.G, color.B)

                                'Console.Write("[{0:000}]rgb:{1:000},{2:000},{3:000}  html:{4} ", i, color.R, color.G, color.B, ToHexValue(color));
                                'if (i%2 != 0) Console.WriteLine();
                            End If
                        Next
                        nusbioPixel.Show()
                        speed = jStep / 2
                        If speed > 0 Then
                            Thread.Sleep(speed)
                        End If
                        CheckKeyboard(quit, speed)
                        If quit Then
                            Exit While
                        End If
                        j += jStep
                    End While
                Next


                For jStep = 24 To 3 Step -2
                    ConsoleEx.WriteLine(0, 2, String.Format("jStep:{0:000}", jStep), ConsoleColor.White)

                    Dim j = 256
                    While j > 0
                        For i As Integer = 0 To nusbioPixel.Count - 1
                            Dim color = RGBHelper.Wheel((i * 256 / nusbioPixel.Count) + j)
                            If i = 0 Then
                                nusbioPixel.SetPixel(i, color.R, color.G, color.B)
                            Else
                                nusbioPixel.SetPixel(color.R, color.G, color.B)

                                'Console.Write("[{0:000}]rgb:{1:000},{2:000},{3:000}  html:{4} ", i, r.R, r.G, r.B, ToHexValue(r));
                                'if (i%2 != 0) Console.WriteLine();
                            End If
                        Next
                        nusbioPixel.Show()
                        speed = jStep / 2
                        If speed > 0 Then
                            Thread.Sleep(speed)
                        End If
                        CheckKeyboard(quit, speed)
                        If quit Then
                            Exit While
                        End If
                        j -= jStep
                    End While
                Next
                ConsoleEx.WriteMenu(0, 4, nusbioPixel.GetByteSecondSentStatus(True))
            End While
        End Sub

        Private Shared Function AskUserForPixelType() As NusbioPixelDeviceType
            Console.Clear()
            ConsoleEx.TitleBar(0, GetAssemblyProduct())
            Dim m = "Pixel Type:  Strip 3)0  Strip 6)0  S)quare 16  R)ing 12"
#If _300_LEDS Then
			m += " 3 H)undred"
#End If
#If _300_LEDS Then
#End If
            Dim pixelTypeChar = ConsoleEx.Question(1, m, New List(Of Char)() From {
                "3"c,
                "6"c,
                "S"c,
                "R"c,
                "H"c
            })
            Select Case pixelTypeChar
                Case "3"c
                    Return NusbioPixelDeviceType.Strip30
                Case "6"c
                    Return NusbioPixelDeviceType.Strip60
                Case "S"c
                    Return NusbioPixelDeviceType.Square16
                Case "R"c
                    Return NusbioPixelDeviceType.Ring12
                Case "H"c
                    Return NusbioPixelDeviceType.Strip300
            End Select
            Return NusbioPixelDeviceType.Unknown
        End Function

        Public Shared Sub Main()

            Dim quit = False
            _rgbLedType = AskUserForPixelType()
            Dim MAX_LED = CInt(_rgbLedType)
            Console.Clear()
            ConsoleEx.TitleBar(0, GetAssemblyProduct())
            Console.WriteLine("")

            Dim nusbioPixel__1 As NusbioPixel = ConnectToMCU(Nothing, MAX_LED)
            If nusbioPixel__1 Is Nothing Then
                Return
            End If

            nusbioPixel__1.SetStrip(Color.Green)
            If nusbioPixel__1.Firmware = Mcu.FirmwareName.NusbioMcu2StripPixels Then
                nusbioPixel__1.SetStrip(Color.Green, stripIndex:=NusbioPixel.StripIndex.S1)
            End If

            Cls(nusbioPixel__1)

            While Not quit
                If Console.KeyAvailable Then
                    Dim k = Console.ReadKey(True).Key
                    If k = ConsoleKey.Q Then
                        quit = True
                    End If
                    If k = ConsoleKey.D0 Then
                        RainbowDemo(nusbioPixel__1, RainbowEffect.AllStrip)
                    End If
                    If k = ConsoleKey.D1 Then
                        RainbowDemo(nusbioPixel__1, RainbowEffect.Spread)
                    End If
                    If k = ConsoleKey.S Then
                        SquareDemo(nusbioPixel__1)
                    End If
                    If k = ConsoleKey.L Then
                        LineDemo(nusbioPixel__1)
                    End If

                    If k = ConsoleKey.I Then
                        nusbioPixel__1 = ConnectToMCU(nusbioPixel__1, MAX_LED).Wait(500).SetStrip(Color.Green)
                    End If
                    Cls(nusbioPixel__1)
                Else
                    ConsoleEx.WaitMS(100)
                End If
            End While
            nusbioPixel__1.Dispose()
        End Sub
    End Class
End Namespace


Module main

    Sub Main()
        NusbioPixelConsole.Program.Main()
    End Sub

End Module