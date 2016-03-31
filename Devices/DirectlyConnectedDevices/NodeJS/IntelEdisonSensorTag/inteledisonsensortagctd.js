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

// We are using the Cylon library to access the hardware resources,'
// and take advantage of its convinient model for running tasks
var Cylon = require('cylon');

// Using connectthedots npm package to connect to IoTHub
var connectthedots = require('connectthedots');
var devicesettings = require('./settings.json');

// Adding sensor tag librarries
var SensorTag = require('./lib/sensortag');

// Keeping track of sensortag connectivity
var SensorTagConnected = false;
var DiscoveringSensorTag = false;

// ---------------------------------------------------------------
// callback for SensorTag discovery
function onDiscover(sensorTag) {
    DiscoveringSensorTag = false;
    console.log('discovered: ' + sensorTag.uuid + ', type = ' + sensorTag.type);
    // Connect the the SensorTag
    sensorTag.connectAndSetUp(function (error) {
        if (error) {
            console.log('ConnectAndSetup Error:' + error);
            SensorTagConnected = false;
        } else {
            // SensorTag connected and setup
            SensorTagConnected = true;

            // Set "disconnect" callback
            sensorTag.on('disconnect', function () {
                console.log('Sensortag disconnected');
                SensorTagConnected = false;
            });

            // Enable IrTemperature sensor, setup 1s period and set callback to send AMQP message to Event Hubs
            sensorTag.enableIrTemperature(function (error) { if (error) console.log('enableIrTemperature ' + error); });
            sensorTag.setIrTemperaturePeriod(1000, function (error) { if (error) console.log('setIrTemperaturePeriod ' + error); });
            sensorTag.notifyIrTemperature(function (error) { if (error) console.log('notifyIrTemperature ' + error); });
            sensorTag.on('irTemperatureChange', function (objectTemperature, ambientTemperature) {
                var currentTime = new Date().toISOString();
                var irObjTemp = (objectTemperature.toFixed(1) * 9) / 5 + 32;
                connectthedots.send_message("IRTemperature", "F", irObjTemp);
            });
            
            // Enable Humidity sensor, setup 1s period and set callback to send AMQP message to Event Hubs
            sensorTag.enableHumidity(function (error) { if (error) console.log('enableHumidity ' + error); });
            sensorTag.setHumidityPeriod(1000, function (error) { if (error) console.log('setHumidityPeriod ' + error); });
            sensorTag.notifyHumidity(function (error) { if (error) console.log('notifyHumidity ' + error); });
            sensorTag.on('humidityChange', function (temperature, humidity) {
                var currentTime = new Date().toISOString();
                var temp = (temperature.toFixed(1) * 9) / 5 + 32;
                var hmdt = humidity.toFixed(1);
                connectthedots.send_message("Temperature", "F", temp);
                connectthedots.send_message("Humidity", "%", hmdt);
            });
        }
    });
}

// ---------------------------------------------------------------
// Let's connect_the_dots
// You can adapt the below code to your own sensors configuration
var connect_the_dots = function()
{
    console.log("Device Ready to connect its dots");
    // ---------------------------------------------------------------
    // Initialization of the Cylon object
    Cylon.robot( {
        connections: {
            edison: { adaptor: 'intel-iot' }
        },
        
        devices: {
            led: { driver: 'led', pin: 13 }
        },
        
        work: function (my) {
            
            // Every second, try and connect to a SensorTag. Toggle Led when discovering. Keep led on when connected
            every((1).second(), function () {

                if (!SensorTagConnected && !DiscoveringSensorTag) {
                    console.log('Discovering sensortag...');
                    DiscoveringSensorTag = true;
                    SensorTag.discover(onDiscover);
                } else {
                    if (DiscoveringSensorTag) my.led.toggle();
                    else if (SensorTagConnected) my.led.turnOn();
                }
            });
        }
    }).start();
}

// Init connection to Azure IoT
connectthedots.init_connection(devicesettings, connect_the_dots );