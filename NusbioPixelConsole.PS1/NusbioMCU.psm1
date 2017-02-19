<#
	NusbioMCU Library For PowerShell

    Copyright (C) 2015,2017 MadeInTheUSB.net
    Written by FT for MadeInTheUSB.net

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
 
$MadeInTheUSB_NusbioMCU_Lib_dll  = "MadeInTheUSB.MCU.Lib.dll"

function pUsing {
    param (
        [System.IDisposable] $obj = $(throw "Parameter -obj is required."),
        [ScriptBlock] $scriptBlock = $(throw "Parameter -scriptBlock is required.")
    )
    Try { 
        &$scriptBlock
    }
    Finally {
        if ($obj -ne $null) {
            
            if ($obj -is [IDisposable]) {

                if ($obj.psbase -eq $null) {
                    $obj.Dispose()
                } else {
                    $obj.psbase.Dispose()
                }
            }
        }
    }
}

function Nusbio_CheckVersion() {

    $t = $PSVersionTable
    Write-Host "PowerShell:$($t.PSVersion), 64BitProcess:$([Environment]::Is64BitProcess), CLR:$($t.CLRVersion), PSCompatibleVersions:$($t.PSCompatibleVersions), Host:$((Get-Host).Version)"
    if($t.PSVersion -lt $Script:POWERSHELL_VERSION) {
        Write-Error "Invalid PowerShell Version"
        Sleep -Seconds 3
        Exit 1
    }
}

function NusbioMCU_Help() {
    Cls
    Write-Host "NusbioMCU " -ForegroundColor Cyan    
}

function Nusbio_GetGpioState($nusbio) {

    $m = ""
    foreach ($gpioIndex in 0,1,2,3,4,5,6,7) {

        $m += "{0}:{1} " -f $gpioIndex, $nusbio.GetGpio("Gpio" + $gpioIndex).PinState.ToString().PadRight(4)
    }
    return $m
}

function Nusbio_AddGpioProperties($nusbio) {

    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio0" -Value $nusbio.GetGPIO("Gpio0")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio1" -Value $nusbio.GetGPIO("Gpio1")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio2" -Value $nusbio.GetGPIO("Gpio2")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio3" -Value $nusbio.GetGPIO("Gpio3")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio4" -Value $nusbio.GetGPIO("Gpio4")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio5" -Value $nusbio.GetGPIO("Gpio5")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio6" -Value $nusbio.GetGPIO("Gpio6")
    Add-Member -InputObject $nusbio -MemberType NoteProperty -Name "Gpio7" -Value $nusbio.GetGPIO("Gpio7")
}

Write-Host "NusbioMCU PowerShell Module Initialization"
Nusbio_CheckVersion
try {
	Write-Host "Loading $MadeInTheUSB_NusbioMCU_Lib_dll"
    Add-Type -ErrorAction Continue -Path $MadeInTheUSB_NusbioMCU_Lib_dll
}
catch [System.Exception] {
    $ex = $_.Exception
    if($ex.GetType().Name  -ine "ReflectionTypeLoadException") {
        Write-Host $ex.ToString()
    }
}

# Create variable to manipulate C# Enum type from the library
#$Gpio0 = [MadeInTheUSB.NusbioGpio]::Gpio0

Export-ModuleMember -function pUsing, NusbioMCU_Help
#Export-ModuleMember -Variable $Gpio0, $Gpio1, $Gpio2, $Gpio3, $Gpio4, $Gpio5, $Gpio6, $Gpio7

