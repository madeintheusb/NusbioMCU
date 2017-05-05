//
// Turning on and off LEDs with Nusbio and F#
//
open System
open System.Threading
#r @"C:\DVT\MadeInTheUSB.MCU\NusbioMCU\MadeInTheUSB.MCU.Lib\bin\Debug\MadeInTheUSB.MCU.Lib.dll"
open MadeInTheUSB
open System.Drawing
open MadeInTheUSB.MCU
open MadeInTheUSB.Components

module NusbioMCUInteractive = 
    let maxLed = 12
    let nusbioPixel = MadeInTheUSB.MCU.NusbioPixel.ConnectToMCU(null, maxLed);
    nusbioPixel.SetStrip(System.Drawing.Color.Black);
    
    let b = System.Drawing.Color.Blue;
    nusbioPixel.SetPixel(0, b);
    [1..29] |> List.iter(fun(x) -> nusbioPixel.SetPixel(x, b) |> ignore);
    nusbioPixel.Show();

    // Based on ADAFRUIT strandtes.ino for NeoPixel
    let WheelByte (wheelPos : int) =
        if (wheelPos < 85) then
            System.Drawing.Color.FromArgb(0, wheelPos*3, 255 - wheelPos*3, 0);
        else if (wheelPos < 170) then
            let wheelPos2 = wheelPos-85;
            System.Drawing.Color.FromArgb(0, 255 - wheelPos2*3, 0, wheelPos2*3);
        else 
            let wheelPos2 = wheelPos - 170;
            System.Drawing.Color.FromArgb(0, 0, wheelPos2*3, 255 - wheelPos2*3);


    //let color = WheelByte((int (byte jWheelColorIndex))); 

    [0..10] |> List.iter(fun(count) ->
        [0..8..256] |> List.iter(fun(jWheelColorIndex) ->
            System.Console.WriteLine("jWheelColorIndex:{0} ", jWheelColorIndex);
            [0..nusbioPixel.Count] |> List.iter(fun(pixelIndex) -> 
                
                let color = WheelByte((int (byte ((pixelIndex * 256 / nusbioPixel.Count) + jWheelColorIndex))));
                if(pixelIndex = 0) then
                    nusbioPixel.SetPixel(pixelIndex, color) |> ignore;
                else
                    nusbioPixel.SetPixel(color) |> ignore;
            )         
            nusbioPixel.Show() |> ignore;
        )
    )

    nusbioPixel.Dispose();