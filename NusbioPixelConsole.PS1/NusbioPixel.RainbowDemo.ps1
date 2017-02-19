<#
	NusbioMCU Demo Using PowerShell

    Copyright (C) 2015 MadeInTheUSB.net

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
#>
 
Param(
  [Alias("a")]   [string] $action = "" # edit to edit sourcecode
) 

cls

if($action.ToLowerInvariant() -eq "sourcecode") 
{
    powershell_ise.exe "NusbioPixel.Console.PowerShell.ps1"
    Exit 0
}

Import-Module ".\NusbioMCU.psm1" -Force


function RainbowDemo([MadeInTheUSB.MCU.NusbioPixel] $nusbioPixel) {
    
    cls
    "Rainbow Demo`r`n"

    $quit                = $false
    $MaxWheelColor       = 256
    $quit                = $false
    $speed               = 5
    $jWheelColorStep     = 4
    $brightness          = 190 # This is for NusbioMCUpixel, will automatically be reduced for NusbioMCU
    $nusbioPixel.SetBrightness($brightness) | Out-Null
    $nusbioPixel.ResetBytePerSecondCounters()
    
    while($quit -eq $false) {

        Write-Host -NoNewline "WheelColorIndex:"
        $sw = [System.Diagnostics.StopWatch]::StartNew()

        for ($jWheelColorIndex = 0; $jWheelColorIndex -lt $MaxWheelColor; $jWheelColorIndex += $jWheelColorStep)  {
            
            Write-Host -NoNewline ("{0:000}," -f $jWheelColorIndex)

            for ($pixelIndex = 0; $pixelIndex -lt $nusbioPixel.Count; $pixelIndex++)  {

                $color = [MadeInTheUSB.Components.RGBHelper]::Wheel(($pixelIndex * 256 / $nusbioPixel.Count) + $jWheelColorIndex)

                if($pixelIndex -eq 0) {
                    $nusbioPixel.SetPixel($pixelIndex, $color) | Out-Null
                }
                else {
                    $nusbioPixel.SetPixel($color) | Out-Null
                }
            }
            $nusbioPixel.Show() | Out-Null
            if($speed -gt 0) { Start-Sleep -Milliseconds $speed }

            if ([console]::KeyAvailable)
            {
                $k = [System.Console]::ReadKey($true) 
                switch ($k.key)
                {
                    "Q" { $quit = $true; $jWheelColorIndex = $MaxWheelColor}
                }
            } 
        }
        $sw.Stop()
        "`r`nSetPixel()/Show() {0}`r`n" -f $nusbioPixel.GetByteSecondSentStatus($true)
    }
}


$MAX_LED = 30

pUsing ($nusbioPixel = [MadeInTheUSB.MCU.NusbioPixel]::ConnectToMCU($null, $MAX_LED)) {

        
    RainbowDemo $nusbioPixel
}
           


#$error[0]|format-list -force
Write-Host "Done"

