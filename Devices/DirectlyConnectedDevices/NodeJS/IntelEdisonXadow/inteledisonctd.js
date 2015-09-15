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

// We are using the johnny-five library to access the hardware resources,'
var five = require('johnny-five');
var Edison = require("edison-io");
var connectthedots = require('./connectthedots.js');
var devicesettings = require('./settings.json');

// ---------------------------------------------------------------
// Let's connect_the_dots
// You can adapt the below code to your own sensors configuration
function connect_the_dots()
{
    console.log("Device Ready to connect its dots");
        
    var temp = 25;
    var acc = 0;
    var nbaccmeasures = 0;

    // Initialize temperature sensor
    var multi = new five.Multi({
        controller: "BMP180"
    });
    
    multi.on("change", function() {
        // console.log("BMP180");
        // console.log("  pressure     : ", this.barometer.pressure);
        // console.log("  temperature  : ", this.temperature.celsius)
        // console.log("--------------------------------------");
        temp = this.temperature.celsius;
        // var currentTime = new Date().toISOString();
        // send_message(format_sensor_data(settings.guid1, settings.displayname, settings.organization, settings.location, "Temperature", "C", currentTime , this.temperature.celsius), currentTime);
    
    });
    
    // Initialize accelerometer    
    var accelerometer = new five.Accelerometer({
        controller: "ADXL345"
    });
    
    accelerometer.on("change", function() {
        // console.log("accelerometer");
        // console.log("  x            : ", this.x);
        // console.log("  y            : ", this.y);
        // console.log("  z            : ", this.z);
        // console.log("  pitch        : ", this.pitch);
        // console.log("  roll         : ", this.roll);
        // console.log("  acceleration : ", this.acceleration);
        // console.log("  inclination  : ", this.inclination);
        // console.log("  orientation  : ", this.orientation);
        // console.log("--------------------------------------");
        acc += this.acceleration;
        nbaccmeasures++ ;
        // var currentTime = new Date().toISOString();
        // send_message(format_sensor_data(settings.guid2, settings.displayname, settings.organization, settings.location, "Acceleration", "G", currentTime , this.acceleration), currentTime);
    });
    
    // send data to Azure every 500 milliseconds    
    setInterval(function(){
        connectthedots.send_message("Temperature", "C", temp);
        if (nbaccmeasures>1)
        {
            acc = acc/nbaccmeasures;
            connectthedots.send_message("Acceleration", "G", acc);
            acc = 0;
            nbaccmeasures = 0;
        }
    }, 500);
}

// ---------------------------------------------------------------
// Init app

// Init connection to Azure IoT
connectthedots.init_connection(devicesettings)

// Init board
var board = new five.Board({io: new Edison(Edison.Boards.Xadow)});

board.on("ready",connect_the_dots);
board.on("message", function(event){
    console.log("Received a %s message, from %s, reporting: %s", event.type, event.class, event.message);
} );
board.on("fail", function(event) {
  console.log("%s sent a 'fail' message: %s", event.class, event.message);
});
board.on("warn", function(event) {
  console.log("%s sent a 'warn' message: %s", event.class, event.message);
});




