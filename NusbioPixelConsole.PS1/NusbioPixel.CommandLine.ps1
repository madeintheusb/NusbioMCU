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
 

cls

Import-Module ".\NusbioMCU.psm1" -Force

$nusbioPixel = [MadeInTheUSB.MCU.NusbioPixel]::ConnectToMCU($null, 30)

$r = $nusbioPixel.SetStrip([System.Drawing.Color]::Black)

$color = [System.Drawing.Color]::DarkOliveGreen
$r = $nusbioPixel.SetPixel([int]0, [System.Drawing.Color]$color)
$r = $nusbioPixel.SetPixel([System.Drawing.Color]$color)
for($i = 0; $i -lt $nusbioPixel.Count-1; $i++) { $r = $nusbioPixel.SetPixel([System.Drawing.Color]$color) }
$r = $nusbioPixel.Show()

$r = $nusbioPixel.Dispose()
           
#$error[0]|format-list -force
Write-Host "Done"






<#
$color = [System.Drawing.Color]::DarkCyan
$color = [System.Drawing.Color]::Crimson
$color = [System.Drawing.Color]::DarkOrange
$color = [System.Drawing.Color]::DarkOliveGreen
#>