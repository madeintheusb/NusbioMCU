/*
   Copyright (C) 2016,2017 MadeInTheUSB LLC

   The MIT License (MIT)

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in
        all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        THE SOFTWARE.
 
    Written by FT for MadeInTheUSB
    MIT license, all text above must be included in any redistribution
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using MadeInTheUSB.MCU;

namespace MadeInTheUSB.NusbioDevice.WebClient.Controllers
{
    public class GpioController : NusbioController
    {
        
    }

    public class NusbioController : ApiController
    {
        private static MadeInTheUSB.MCU.NusbioPixel _nusbioPixel;

        private const int MAX_LED = 30;

        public static bool InitNusbioDevice()
        {
            _nusbioPixel = NusbioPixel.ConnectToMCU(null, MAX_LED);
            _nusbioPixel.SetBrightness(200);
            return true;
        }

        private const string NUSBIO_MCU_DEVICE_NOT_DETECTED = "NusbioMCU device not detected";

        private static bool NusbioMcuDeviceAvailable()
        {
            return _nusbioPixel != null;
        }

        [HttpGet]
        /// <summary>
        /// http://localhost:55977/api/Nusbio
        /// http://localhost:55977/api/nusbio/blue
        /// http://localhost:55977/api/nusbio/red
        /// </summary>
        /// <returns></returns>
        public string Get(string p1)
        {
            return Get(p1, null);
        }
        public string Get(string p1, string p2)
        {
            if(p1 != null) p1 = p1.ToLowerInvariant();
            if(p2 != null) p2 = p2.ToLowerInvariant();

            if (!NusbioMcuDeviceAvailable())
                return NUSBIO_MCU_DEVICE_NOT_DETECTED; 

            var uri = base.Request.RequestUri.AbsoluteUri;
            var ok  = false;

            if (p1 == "setpixels")
            {
                ok         = true;
                var colors = p2.Split(',').ToList();
                for(var i=0; i < colors.Count; i++)
                {
                    try
                    {
                        var wx = int.Parse(colors[i]);
                        _nusbioPixel.SetPixel(i, MadeInTheUSB.Components.RGBHelper.Wheel(wx), optimized: true);
                        //_nusbioPixel.SetPixel(i, System.Drawing.ColorTranslator.FromHtml("#" + colors[i]), optimized:true);
                    }
                    catch(System.Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                _nusbioPixel.Show();
                return PrepareResponseOk(uri);
            }
            else if (p1 == "setstrip")
            {
                ok = true;
                _nusbioPixel.SetStrip(System.Drawing.ColorTranslator.FromHtml("#"+p2), _nusbioPixel.DEFAULT_BRIGHTNESS);
                return PrepareResponseOk(uri);
            }
            else if (p1 == "setledcount")
            {
                ok = true;
                _nusbioPixel.SetLedCount(int.Parse(p2));
                return PrepareResponseOk(uri);
            }
            else if (p1 == "getdevicestate")
            {
                ok = true;
                return PrepareResponse(new { Count = _nusbioPixel.Count, Url = uri, Succeeded = ok.ToString().ToLowerInvariant() }, ok);
            }
            else return PrepareResponseOk(uri, false);
        }

        private string PrepareResponseOk(string url, bool ok = true)
        {
            return PrepareResponse(new { Count = _nusbioPixel.Count, Url = url, Succeeded = ok.ToString().ToLowerInvariant() }, ok);
        }

        private string PrepareResponse(object response, bool succeed)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented);
            return json;
        }

        void TraceUrl(string url)
        {
            Debug.WriteLine(url);
        }
       
    }
}


