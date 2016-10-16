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

var clientFromConnectionString = require('azure-iot-device-mqtt').clientFromConnectionString;
var Message = require('azure-iot-device').Message;

// Using a json settings file for Events Hub connectivity
var devicesettings;

// Iot Hub client instance
var client;

var is_connected = false;
var init_callback;
var receive_callback;

// ---------------------------------------------------------------
// validate settings from JSON file  passed as a parameter to the app
function validate_settings(settings, options) {
    console.log("Validating device settings");
    var missing = [];
    for (var idx in options) {
        if (settings[options[idx]] === undefined) missing.push(options[idx]);
    }
    if (missing.length > 0) {
        // app is terminated if settings are missing
        throw new Error('Required settings ' + (missing.join(', ')) + ' missing.');
    }
}


// ---------------------------------------------------------------
// Format sensor data into JSON
function format_sensor_data(deviceid, displayname, organization, location, measurename, unitofmeasure, timecreated, value) {
    var JSON_obj = {
        "guid": deviceid,
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
// Initializes connection settings to Event Hub
var connectCallback = function (err) {
  if (err) {
    console.error('Could not connect: ' + err.message);
  } else {
    console.log('IoT Hub Client connected');

    client.on('message', function (msg) {
      
      // Invoke receive callback to notify the user of a new message
      if (receive_callback!==undefined) {
          receive_callback(msg);
      }  

      client.complete(msg, printResultFor('completed'));
      // TODO:Implement sending the received message up to the connectthedots user
      // reject and abandon follow the same pattern.
      // /!\ reject and abandon are not available with MQTT
    });

    client.on('error', function (err) {
      console.error(err.message);
    });

    client.on('disconnect', function () {
      is_connected = false;
      client.removeAllListeners();
      client.connect(connectCallback);
    });
    
    is_connected = true;
    init_callback();
  }
}

exports.init_connection = function(settings, initCallback, receiveCallback)
{
    console.log("Initializig the connection with Azure IoT Hub");
    devicesettings = settings;
    
    console.log("Validating connection settings");
    // Validate settings
    validate_settings(devicesettings, ['deviceid', 'iothubconnectionstring', 'displayname', 'organization', 'location']);

    console.log("Connecting to Azure IoT Hub");
    // Create Iot Hub Client instance 
    client = clientFromConnectionString(devicesettings.iothubconnectionstring);
    init_callback = initCallback;

    if (receiveCallback!==undefined) receive_callback = receiveCallback;  
    
    // Open the transport stack
    client.open(connectCallback);
};


function send_raw_message(raw_message)
{
    if (is_connected)
    {
        var message = new Message(raw_message);
        console.log('Sending message: ' + message.getData());
        client.sendEvent(message, printResultFor('send'));
    } else
    {
        console.log("Transport channel is not opened. Trying to open it...");
    }
    
}

// ---------------------------------------------------------------
// Send message to Event Hub
exports.send_message = function(measurename, unitofmeasure, value)
{
    var currentTime = new Date().toISOString();
    var message = format_sensor_data(devicesettings.deviceid, devicesettings.displayname, devicesettings.organization, devicesettings.location, measurename, unitofmeasure, currentTime, value);
	console.log("Sending message: " + message);
    
    send_raw_message(message);
}

// ---------------------------------------------------------------
// Send bulk messages to Event Hub
exports.send_bulk_message = function(messages)
{
    var currentTime = new Date().toISOString();
    var message;
    
    messages.forEach( function(element, index, array){
        message+= format_sensor_data(devicesettings.deviceid, devicesettings.displayname, devicesettings.organization, devicesettings.location, element.measurename, element.unitofmeasure, currentTime, element.value);
    }); 
	console.log("Sending bulk message: " + message);
    
    send_raw_message(message);
}

// Helper function to print results in the console
function printResultFor(op) {
  return function printResult(err, res) {
    if (err) console.log(op + ' error: ' + err.toString());
    if (res) console.log(op + ' status: ' + res.constructor.name);
  };
}


