'use strict'

// Using HTTP Rest connection to Azure event Hubs
//var https = require('https');
var crypto = require('crypto');
var moment = require('moment');
var request = require('request');
var _ = require('lodash');

// Sensortag utility class
var worker = require('./sensorWorker.js');

// SETTTINGS
// Using a json settings file for Events Hub connectivity
var settings = require('./settings.json');

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

//AUTH & SEND
// Format sensor data into JSON
function prepareData(data) {
	var metaData = {
        "guid": settings.guid,
        "displayname": settings.displayname,
        "organization": settings.organization,
        "location": settings.location
    };
    var preparedData = _.merge(data, metaData);
    return JSON.stringify(preparedData);
}

// ---------------------------------------------------------------
// Get the full Event Hub publisher URI
var myUri = 'https://' + settings.namespace + '.servicebus.windows.net' + '/' + settings.eventhubname + '/publishers/' + settings.guid + '/messages';

// ---------------------------------------------------------------
// Create a SAS token
// See http://msdn.microsoft.com/library/azure/dn170477.aspx
function createSASToken(uri, key_name, key) {
    // Token expires in one hour
    var expiry = moment().add(1, 'hours').unix();
    var string_to_sign = encodeURIComponent(uri) + '\n' + expiry;
    var hmac = crypto.createHmac('sha256', key);
    hmac.update(string_to_sign);
    var signature = hmac.digest('base64');
    var token = 'SharedAccessSignature sr=' + encodeURIComponent(uri) + '&sig=' + encodeURIComponent(signature) + '&se=' + expiry + '&skn=' + key_name;        
    return token;
}

var mySas = createSASToken(myUri, settings.keyname, settings.key);

// Send message to Event Hub
function sendMessage(message) {
	console.log("Sending message: " + message);

    var headers = {
        'Authorization': mySas,
        'Content-Length': message.length,
        'Content-Type': 'application/atom+xml;type=entry;charset=utf-8'
    };

    request.post(myUri, {'headers':headers, 'body': message}, function(err, res, body){
    	if (!err) 
			console.log(res.statusCode, body)
		else console.error(err);
    });
}

// Start sensor worker
worker.start(function(data){
	var formedData = prepareData(data, settings);
	sendMessage(formedData);
});
