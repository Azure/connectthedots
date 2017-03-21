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
var tessel = require('tessel');
var ambientlib = require('ambient-attx4');

// main function to initialize the Tessel and start reading data
var connect_the_dots = function () {
  // Connect to our ambient sensor.
  var ambient = ambientlib.use(tessel.port['A']);

  console.log('Tessel 2 is ready to connect its dots');

  // Read light and sound data at a 1 second interval
  setInterval(function () {
    // read some sound level data
    ambient.getSoundLevel(function (err, sdata) {
      if (err) throw err;
      console.log('sound: ', sdata)
      // read some light level data
      ambient.getLightLevel(function (err, ldata) {
        if (err) throw err;
        console.log('light: ', ldata);

        // send data to IoT Hub
        connectthedots.send_message("Sound", "units", sdata);
        connectthedots.send_message("Light", "units", ldata);
      });
    });
  }, 1000);
}

var initCallback = function (err) {
  // Once the connection to Azure IoT Hub is establish you can initialize your hardware and start sending data
  // This is where you would insert your sensors code
  connect_the_dots();
};

var receiveCallback = function (msg) {
  // A message was received
  console.log("Received Message. Message Id: " + msg.messageId + " ; Message data:" + msg.data.toString() );
};


// ---------------------------------------------------------------
// Init app

// Init connection to Azure IoT
connectthedots.init_connection(devicesettings, initCallback, receiveCallback);
