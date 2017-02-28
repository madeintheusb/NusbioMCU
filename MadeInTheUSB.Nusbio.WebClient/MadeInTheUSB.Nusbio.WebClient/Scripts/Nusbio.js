/*
    Copyright (C) 2016 MadeInTheUSB.net

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

var
    Gpio4 = 4, 
    Gpio5 = 5, 
    Gpio6 = 6, 
    Gpio7 = 7;

var URL_API_PREFIX = "/api"; // Not need for the Nusbio Windows console with Nancy

function NusbioClass(host, hostPort, getCallBack) {

    this._traceOn  = false;
    this.GpioNames = ["Gpio4", "Gpio5", "Gpio6", "Gpio7"];
    var $this      = this;

    this.__init__ = function() {

        if (!getCallBack)
            throw "parameter getCallBack is required";
        this._baseUrl = this.__stringFormat("http://{0}:{1}{2}", host, hostPort, URL_API_PREFIX);
        this.__userCallBack = getCallBack;
        this.Count = 30;
    }
    this.__get = function(url, data, successFunction) {
        try {
            this.__trace(this.__stringFormat("GET url:{0}, data:{1}", url, data));
            jQuery.get(url, data, successFunction);
        } catch (ex) {
            this.__trace(this.__stringFormat("Cannot contat Nusbio web server at {0}, error:{1}", url, ex));
            throw ex;
        }
    }
    this.__callNusbioRestApi = function(url, callBack) {
                
        if (typeof(callBack) === 'undefined')
            callBack = this.__getCallBack;

        //this.__trace(url);
        console.log(url); // this make a better demo

        var url = this._baseUrl + url;
        this.__get(url, "", callBack);
    }
    this.__getCallBack = function(result) {
        try {
            //alert(result);
            var r = JSON.parse(result); // Result is an array of ExecuteScriptItemResult only get the firstone
            $this.__userCallBack(r);
        } 
        catch (ex) {
            $this.__trace($this.__stringFormat("Error calling callback:{0}", ex));
            throw ex;
        }
    }
    this.__trace = function(m) {

        if(this._traceOn)
            console.log("NusbioWebSite:" + m.toString());
        return m;
    }
    this.setLedCount = function (count) {

        this.Count = count;
        this.__callNusbioRestApi(this.__stringFormat("/nusbio/setledcount/{0}", count));
    }
    this.getDeviceState = function () {

        this.__callNusbioRestApi("/nusbio/getdevicestate");
    }
    this.setStrip = function (color) {

        this.__callNusbioRestApi(this.__stringFormat("/nusbio/setstrip/{0}", color));
    }
    this.setPixels = function (csvColors) {

        this.__callNusbioRestApi(this.__stringFormat("/nusbio/setpixels/{0}", encodeURIComponent(csvColors)));
    }
    this.fromRgb = function (r, g, b) {

        //if (b < 0) alert("b:" + b);
        //return toHexa(Math.floor(r)) + toHexa(Math.floor(g)) + toHexa(Math.floor(b));
        return this.__stringFormat("{0}{1}{2}",
            toHexa(Math.floor(r)), toHexa(Math.floor(g)), toHexa(Math.floor(b)));
    }
    function toHexa(n) {

        var s = n.toString(16);
        if (s.length == 1)
            s = "0" + s;
        return s;
    }
    // Based on ADAFRUIT strandtes.ino for NeoPixel
    // Input a value 0 to 255 to get a color value.
    // The colours are a transition r - g - b - back to r.
    this.wheel = function (wheelPos) {

        wheelPos = Math.floor(wheelPos);

        if (wheelPos < 85) {
            return this.fromRgb(wheelPos * 3, 255 - wheelPos * 3, 0);
        }
        else if (wheelPos < 170) {
            wheelPos -= 85;
            return this.fromRgb(255 - wheelPos * 3, 0, wheelPos * 3);
        }
        else {
            wheelPos -= 170;
            return this.fromRgb(0, wheelPos * 3, 255 - wheelPos * 3);
        }
    }
    this.__stringFormat = function () {
        ///	<summary>
        ///Format the string passed as first argument based on the list of following arguments referenced in the format template&#10;
        ///with the synatx {index} 
        ///Sample:&#10;
        ///     var r = "LastName:{0}, Age:{1}".format("TORRES", 45);&#10;
        ///	</summary>    
        ///	<param name="tpl" type="string">value</param>
        ///	<param name="p1" type="object">value</param>
        for (var i = 1; i < arguments.length; i++)
            arguments[0] = arguments[0].replace(new RegExp('\\{' + (i - 1) + '\\}', 'gm'), arguments[i]);
        return arguments[0];
    }
    this.__init__();
}