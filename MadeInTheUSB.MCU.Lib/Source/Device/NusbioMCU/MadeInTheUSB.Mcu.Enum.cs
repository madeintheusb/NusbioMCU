/*
    NusbioMatrix/NusbioPixel devices for Windows/.NET
    MadeInTheUSB MCU ATMega328 Based Device
    Copyright (C) 2016, 2017, 2019 MadeInTheUSB LLC

    MIT license, all text above must be included in any redistribution

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
using System.Text;

namespace MadeInTheUSB.MCU
{
    public class Mcu
    {
        public const int CP_RGB_PIXEL_2_STRIP_CMD_OFFSET = 220;
        /// <summary>
        /// Common api to all the MadeInTheUSB MCU ATMega328 based device 
        /// </summary>
        public enum McuCommand
        {
            CP_GETFIRMWARE_NAME = 0,
            CP_SETDEVICECOUNT   = 1,
            CP_PING             = 2,
            CP_DRAWFRAME        = 3,
            CP_SCROLLRIGHT      = 4,
            CP_SCROLLLEFT       = 5,
            CP_GETDEVICECOUNT   = 6,
            CP_SETINTENSITY     = 7,
            CP_DRAWROW          = 8,
            CP_ONBOAD_LED       = 9,
            CP_ANALOG_READ      = 10,
            CP_DIGITAL_READ     = 11,
            CP_SET_PIN_MODE     = 12,
            CP_DIGITAL_WRITE    = 13,
            CP_ANALOG_WRITE     = 14,

            CP_RGB_PIXEL_SET_COUNT               = 20, // Follow by an int (byte 0, byte 1)
            CP_RGB_PIXEL_SET_COLOR_1BYTE_INDEX   = 21, // 1 byte for pixel index, r,g,b as byte. Index is less than 256.
            CP_RGB_PIXEL_SET_COLOR_NO_INDEX      = 22, // r,g,b as byte, inc last index position
            CP_RGB_PIXEL_SET_COLOR_2BYTE_INDEX   = 23, // 2 bytes for pixel index, r,g,b as byte
            CP_RGB_PIXEL_DRAW                    = 24, // Refresh strip
            CP_RGB_PIXEL_SET_BRIGTHNESS          = 25,

            CP_RGB_PIXEL_2_SET_COUNT             = 20 + CP_RGB_PIXEL_2_STRIP_CMD_OFFSET, // Follow by an int (byte 0, byte 1)
            CP_RGB_PIXEL_2_SET_COLOR_1BYTE_INDEX = 21 + CP_RGB_PIXEL_2_STRIP_CMD_OFFSET, // 1 byte for pixel index, r,g,b as byte. Faster is index is less than 256
            CP_RGB_PIXEL_2_SET_COLOR_NO_INDEX    = 22 + CP_RGB_PIXEL_2_STRIP_CMD_OFFSET, // r,g,b as byte, inc last index position
            CP_RGB_PIXEL_2_SET_COLOR_2BYTE_INDEX = 23 + CP_RGB_PIXEL_2_STRIP_CMD_OFFSET, // 2 byte for pixel index, r,g,b as byte
            CP_RGB_PIXEL_2_DRAW                  = 24 + CP_RGB_PIXEL_2_STRIP_CMD_OFFSET, // Refresh strip
            CP_RGB_PIXEL_2_SET_BRIGTHNESS        = 25 + CP_RGB_PIXEL_2_STRIP_CMD_OFFSET,

            // EEPROM 25AA1024 size:128kByte page.size:256, page.count:512
            CP_EEPROM_GET_INFO    = 30, // Return page.size/4, page.count/4
            CP_EEPROM_SET_ADDR    = 31, // Set address for next read or write operation
                                     // Address is a 16 bit value
            CP_EEPROM_WRITE_PAGES = 32, // Param0 is 0, We only write one page at the time
                                        // followed by 256 bytes or 1 page. 5 ms wait time required
            CP_EEPROM_READ_PAGES  = 33, // Param0 is number of page to read (ord send for MCU to PC)
                                       // Let's start with each page of 256b is sent in one operation
                                       // with a wait time of 2 ms second to let the PC get the data

            CP_TEST_SIMULATOR     = 26,
        }
        /// <summary>
        /// All MadeInTheUSB MCU ATMega328 based devices 
        /// </summary>
        public enum FirmwareName
        {
            Unknown               = 0,
            NusbioMcuMatrixPixel  = 1,
            NusbioMcuEeprom       = 2, // Not used
            NusbioMcu2StripPixels = 4,
        }

        public enum DigitalIOMode
        {
            INPUT        = 0x0,
            OUTPUT       = 0x1,
            INPUT_PULLUP = 0x2
        }

        public enum GpioPin
        {
            Gpio4 = 4,
            Gpio5 = 5,
            Gpio6 = 6,
            Gpio7 = 7
        };

        public enum GpioPwmPin
        {
            Gpio5 = 5,
            Gpio6 = 6,
        }

        public static List<GpioPwmPin> PwmGpioPins = new List<GpioPwmPin>()
        {
            GpioPwmPin.Gpio5,
            GpioPwmPin.Gpio6,
        };

        public enum AdcPin
        {
            Adc4 = 4,
            Adc5 = 5,
            Adc6 = 6,
            Adc7 = 7
        };
    }
}
