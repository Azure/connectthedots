//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

var connectthedots = require('connectthedots');
var devicesettings = require('./settings.json');

// ---------------------------------------------------------------
// Let's connect_the_dots
// You can adapt the below code to your own sensors configuration
var connect_the_dots=function()
{
    console.log("Device Ready to connect its dots");

    var lght = 10;
    var temp = 25;

    // send data to Azure every 1000 milliseconds    
    setInterval(function(){
        lght = lght + (Math.random()*2 -1);
        if (lght < 0 ) lght = 0;
        temp = temp + (Math.random()*2 -1);
        if (temp < 0 ) temp = 0;
        connectthedots.send_message("Light", "L", lght);
        connectthedots.send_message("Temp", "C", temp);
    }, 1000);

};

var initCallback = function (err) {
    // Once the connection to Azure IoT Hub is establish you can initialize your hardware and start sending data
    // This is where you would insert your sensors code
    connect_the_dots();
};

// ---------------------------------------------------------------
// Init app

// Init connection to Azure IoT
connectthedots.init_connection(devicesettings, initCallback);





