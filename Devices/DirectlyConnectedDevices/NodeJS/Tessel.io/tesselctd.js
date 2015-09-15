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


// Libraries for connection to Tessel board and sensor modules
var tessel = require('tessel');

var climatelib = require('climate-si7020');
var climate = climatelib.use(tessel.port['B']);

var ambientlib = require('ambient-attx4');
var ambient = ambientlib.use(tessel.port['A']);

var TesselWifi = require('tessel-wifi');

var connectthedots = require('connectthedots');

var devicesettings = {
    namespace: '[namespace]',
    keyname: '[keyname]',
    key: '[key]',
    eventhubname: 'ehdevices',
    guid: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',
    displayname: 'Tessel',
    organization: 'My Org',
    location:  'My Location'
};

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

var wifi = new TesselWifi(wifiSettings);
 
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


// Init Connection to Azure
// NOTE: we cannot use the below SAS token generation code as the Tessel runtime doesn't support sha256 encryption yet.
// As soon as this is the case, you will be able to generate the SAS token on the flight vs. using the RedDog tools
// Using Red Dog tools to generate SAS token (https://github.com/sandrinodimattia/RedDog/releases/tag/0.2.0.1)
connectthedots.init_connection(devicesettings, "SAS_TOKEN");

var data_temp = 25;
var data_hmdt = 0;
var data_light = 0;
var data_sound = 0;

// ---------------------------------------------------------------
// Once connected to sensor, get readings every second and send to event hub
climate.on('ready', function () {
  console.log('Connected to climate-si7020 sensor');
  setInterval( function () {
    climate.readTemperature('f', function (err, temp) {
      climate.readHumidity(function (err, humid) {
        console.log('Degrees:', temp.toFixed(4) + 'F', 'Humidity:', humid.toFixed(4) + '%RH');
        data_temp = (temp - 32 ) * 5/9;
        data_hmdt = humid;
        led1.toggle();
      });
    });
  }, 3000);
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
        led2.toggle();
        data_light = ldata;
        data_sound = sdata;
      });
    });
  }, 3000);
});

ambient.on('error', function(err) {
  console.log('error connecting module', err);
});

setInterval(function(){
  if (isWifiConnected)
  {
    connectthedots.send_message("Temperature", "F", data_temp);
    setTimeout(function()
    {
      connectthedots.send_message("Humidity", "%", data_hmdt);
      setTimeout(function()
      {
        connectthedots.send_message("Light", "L", data_light);
        setTimeout(function()
        {
          connectthedots.send_message("Sound", "Db", data_sound);
        }, 1000);
      }, 1000);
    }, 1000);
  }
}, 4000);
