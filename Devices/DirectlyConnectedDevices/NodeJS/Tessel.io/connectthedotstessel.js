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

// settings for Azure connectivity
// Replace the below with the settings for the EventHub you want to used
// usually generated with the AzurePrep tool
// Note that the key should NOT be URL-encoded
var settings = {
    namespace: '[namespace]',
    keyname: '[keyname]',
    key: '[key]',
    eventhubname: 'ehdevices',
    guid: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',
    displayname: 'Tessel',
    organization: 'MS Open Tech',
    location:  'Here'
};

// Libraries for connection to Tessel board and sensor modules
var tessel = require('tessel');

var climatelib = require('climate-si7020');
var climate = climatelib.use(tessel.port['B']);

var ambientlib = require('ambient-attx4');
var ambient = ambientlib.use(tessel.port['A']);

var tesselWifi = require('tessel-wifi');

var led1 = tessel.led[0].output(1);
var led2 = tessel.led[1].output(0);

// ---------------------------------------------------------------
// Wifi connection
var isWifiConnected = false;

var wifiSettings = {
  ssid: '[SSID]',
  password: '[Key]',
  security: 'wpa2',
  timeout: 30
};

var wifi = new tesselWifi(wifiSettings);
 
wifi.on('connect', function(err, data){
    //this event gets called whenever we connect or reconnect 
    console.log("Connected to", wifiSettings.ssid);
    isWifiConnected = true;
  })
  .on('disconnect', function(err, data){
    // pause the program here, wait until we reconnect 
    console.log("WARN: No longer connected to", wifiSettings.ssid);
  })
  .on('error', function(err){
    console.log("ERROR:", err);
 
    wifi.reconnect();
  });

// ---------------------------------------------------------------
// Format sensor data into JSON top send to event hub
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

// NOTE: Tessel uses the Lua runtime which has limitations preventing from using the AMQP10 library...
// We are keeping the code commented until limitations are gone or workaround is found
// We are using HTTP/REST instead of AMQP connection
// Library for AMQP connection to Azure Event Hubs
//var AMQPClient = require('amqp10');
// var eventHubClient = require('event-hub-client').restClient(
//   settings.namespace,
//   settings.eventhubname,
//   settings.keyname,
//   settings.key
// );

// ---------------------------------------------------------------
// Get the full Event Hub publisher AMQP URI
//var uri = 'amqps://' + encodeURIComponent(settings.keyname) + ':' + encodeURIComponent(settings.key) + '@' + settings.namespace + '.servicebus.windows.net';
//var client = new AMQPClient(AMQPClient.policies.EventHubPolicy);

// // -----------------------------------------------------------------
// // Send message to Event Hub using event hub client
// function send_message(message, time)
// {
//     if (!isWifiConnected){
//         console.log('Wifi is not connected yet. Cannot send message.');
//     } else
//     {
//         console.log('Sending message with value ' + message);
//         eventHubClient.sendMessage(message);
//     }
// }


// Libraries for HTTP Rest connection to Azure event Hubs
var https = require('https');
var crypto = require('crypto');

// ---------------------------------------------------------------
// Get the full Event Hub publisher URI
var my_uri = 'https://' + settings.namespace + '.servicebus.windows.net' + '/' + settings.eventhubname + '/publishers/' + settings.guid + '/messages';
console.log('Azure Event Hub URL: ' + my_uri);

// NOTE: we cannot use the below SAS token generation code as the Tessel runtime doesn't support sha256 encryption yet.
// As soon as this is the case, you will be able to generate the SAS token onthe flight vs. using the RedDog tools

// // ---------------------------------------------------------------
// // Create a SAS token
// // See http://msdn.microsoft.com/library/azure/dn170477.aspx
// function create_sas_token(uri, key_name, key) {
//    // Token expires in one hour
//    var expiry = Math.round((new Date().getTime() )/1000) + 3600;
//    
//    var string_to_sign = encodeURIComponent(uri) + '\n' + expiry.toString();
//    var hmac = crypto.createHmac('sha256', key);
//    hmac.update(string_to_sign);
//        
//    var signature = hmac.digest('base64');
//        
//    var token = 'SharedAccessSignature sr=' + encodeURIComponent(uri) + '&sig=' + encodeURIComponent(signature) + '&se=' + expiry + '&skn=' + key_name;
//        
//    return token;
// 
// }
// var my_sas = create_sas_token(my_uri, settings.keyname, settings.key);
// console.log('SAS token: ' + my_sas);

// Using Red Dog tools to generate SAS token (https://github.com/sandrinodimattia/RedDog/releases/tag/0.2.0.1)
var my_sas = "[SAS Token]";
console.log('SAS token: ' + my_sas);


// ---------------------------------------------------------------
// Send message to Event Hub using HTTP
function send_message(message, time)
{
    if (!isWifiConnected){
        console.log('Wifi is not connected yet. Cannot send message.');
    } else
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
                console.log(d);
            });
        });
            
        req.on('error', function (e) {
            console.error(e);
        });
            
        req.write(message);
        
        req.end();
    }
}


// ---------------------------------------------------------------
// Once connected to sensor, get readings every second and send to event hub
climate.on('ready', function () {
  console.log('Connected to climate-si7020 sensor');
  setInterval( function () {
    climate.readTemperature('f', function (err, temp) {
      climate.readHumidity(function (err, humid) {
        console.log('Degrees:', temp.toFixed(4) + 'F', 'Humidity:', humid.toFixed(4) + '%RH');

        var currentTime = new Date().toISOString();
        send_message(format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Temperature", "F", currentTime , temp) +
          format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Humidity", "%", currentTime , humid), currentTime);
        led1.toggle();
      });
    });
  }, 1000);
});

climate.on('error', function(err) {
  console.log('error connecting module', err);
});

// ---------------------------------------------------------------
// Once connected to sensor, get readings every second and send to event hub
ambient.on('ready', function () {
  console.log('Connected to ambient-attx4 sensor');
  setInterval( function () {
    ambient.getLightLevel( function(err, ldata) {
      if (err) throw err;
      ambient.getSoundLevel( function(err, sdata) {
        if (err) throw err;
        console.log("Light level:", ldata.toFixed(8) + 'Lumen', " ", "Sound Level:", sdata.toFixed(8) + 'Db');

        var currentTime = new Date().toISOString();
        send_message(format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Light", "Lumen", currentTime , ldata)+ 
          format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Sound", "Db", currentTime , sdata), currentTime);
        led2.toggle();
      });
    });
  }, 1000);
});

ambient.on('error', function(err) {
  console.log('error connecting module', err);
});

