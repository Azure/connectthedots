'use strict'

// Common logic
var connectthedots = require('connectthedots');
// Settings
var devicesettings = require('./settings.json');

// Sensortag utility class
var worker = require('./sensorWorker.js');

var initCallback = function (err) {
    // Once the connection to Azure IoT Hub is establish you can initialize your hardware and start sending data
    // This is where you would insert your sensors code
// Start sensor worker with the send_message callback to the data reception
    worker.start(connectthedots.send_message);
};


// Init connection to Azure IoT
connectthedots.init_connection(devicesettings, initCallback);



