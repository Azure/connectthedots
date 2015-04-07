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

// Using HTTP Rest connection to Azure event Hubs
var https = require('https');
var crypto = require('crypto');
var moment = require('moment');

// Using a json settings file for Events Hub connectivity
var settings = require('./settings.json');
var SensorTag = require('./lib/sensortag');

// Keeping track of sensortag connectivity
var SensorTagConnected = false;
var DiscoveringSensorTag = false;

// ---------------------------------------------------------------
// Read settings from JSON file  passed as a parameter to the app
function readSettings(settings, options) {
    var missing = [];
    for (var idx in options) {
        if (settings[options[idx]] === undefined) missing.push(options[idx]);
    }
    if (missing.length > 0) {
        throw new Error('Required settings ' + (missing.join(', ')) + ' missing.');
    }
}

readSettings(settings, ['namespace', 'keyname', 'key', 'eventhubname', 'displayname', 'guid', 'organization', 'location']);

// ---------------------------------------------------------------
// Get the full Event Hub publisher URI
var my_uri = 'https://' + settings.namespace + '.servicebus.windows.net' + '/' + settings.eventhubname + '/publishers/' + settings.guid + '/messages';
    
// ---------------------------------------------------------------
// Create a SAS token
// See http://msdn.microsoft.com/library/azure/dn170477.aspx
function create_sas_token(uri, key_name, key) {
    // Token expires in one hour
    var expiry = moment().add(1, 'hours').unix();
    var string_to_sign = encodeURIComponent(uri) + '\n' + expiry;
    var hmac = crypto.createHmac('sha256', key);
    hmac.update(string_to_sign);
        
    var signature = hmac.digest('base64');
        
    var token = 'SharedAccessSignature sr=' + encodeURIComponent(uri) + '&sig=' + encodeURIComponent(signature) + '&se=' + expiry + '&skn=' + key_name;
        
    return token;

}
var my_sas = create_sas_token(my_uri, settings.keyname, settings.key);

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
                send_message(format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "IRTemperature", "F", currentTime , irObjTemp), currentTime);
            });
            
            // Enable Humidity sensor, setup 1s period and set callback to send AMQP message to Event Hubs
            sensorTag.enableHumidity(function (error) { if (error) console.log('enableHumidity ' + error); });
            sensorTag.setHumidityPeriod(1000, function (error) { if (error) console.log('setHumidityPeriod ' + error); });
            sensorTag.notifyHumidity(function (error) { if (error) console.log('notifyHumidity ' + error); });
            sensorTag.on('humidityChange', function (temperature, humidity) {
                var currentTime = new Date().toISOString();
                var temp = (temperature.toFixed(1) * 9) / 5 + 32;
                var hmdt = humidity.toFixed(1);
                send_message(format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Temperature", "F", currentTime , temp), currentTime);
                send_message(format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Humidity", "%", currentTime , hmdt), currentTime);
            });
        }
    });
}

// ---------------------------------------------------------------
// Format sensor data into JSON
function format_sensor_data(guid, displayname, organization, location, measurename, unitofmeasure, timecreated, value) {
    var JSON_obj = {
        "guid": guid,
        "displayname": displayname,
        "organization": organization,
        "location": location,
        "measurename": measurename,
        "unitofmeasure": unitofmeasure,
        "timecreated": timecreated,
        "value": value
    };
    
    return JSON.stringify(JSON_obj);
}

// ---------------------------------------------------------------
// Send message to Event Hub
function send_message(message, time)
{
	console.log("Sending message: " + message);
    
    // Send the request to the Event Hub
    var http_options = {
            
        hostname: settings.namespace + '.servicebus.windows.net',
        port: 443,
        path: '/' + settings.eventhubname + '/publishers/' + settings.guid + '/messages',
        method: 'POST',
        headers: {
            'Authorization': my_sas,
            'Content-Length': message.length,
            'Content-Type': 'application/atom+xml;type=entry;charset=utf-8'
        }
    };
        
    var req = https.request(http_options, function (res) {
        console.log("statusCode: ", res.statusCode);
        console.log("headers: ", res.headers);
            
        res.on('data', function (d) {
            process.stdout.write(d);
        });
    });
        
    req.on('error', function (e) {
        console.error(e);
    });
        
    req.write(message);
        
    req.end();
}

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
        
        // Regenerate the SAS token every hour
        every((3600000).second(), function () {
            my_sas = create_sas_token(my_uri, settings.keyname, settings.key);
        });
        
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


