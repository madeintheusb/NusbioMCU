@echo off
cls
set ARDUINO_IDE=C:\DVT\Arduino\Arduino-1.6.9
set FIRMWARE=C:\DVT\MadeInTheUSB.MCU\NusbioMCU\NusbioFirmwareLoader\Firmware\v11\NusbioMatrixATMega328.ino.hex
set avrdude=%ARDUINO_IDE%\hardware\tools\avr\bin\avrdude.exe
set COMPORT=COM3
set avrdude_conf=%ARDUINO_IDE%\hardware\tools\avr\etc\avrdude.conf

Echo Upload Nusbio Firmware ?
Echo PORT: %COMPORT%
Echo FIRMWARE: %FIRMWARE%
pause

echo Command Line
echo "%avrdude%" -C%avrdude_conf% -v -patmega328p -carduino -P%COMPORT% -b57600 -D -Uflash:w:%FIRMWARE%:i
pause

"%avrdude%" -C%avrdude_conf% -v -patmega328p -carduino -P%COMPORT% -b57600 -D -Uflash:w:%FIRMWARE%:i

echo done
