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

// Using HTTP Rest connection to Azure event Hubs
var https = require('https');
var crypto = require('crypto');
var moment = require('moment');

// Using a json settings file for Events Hub connectivity
var settings = require('./settings.json');

// Variables
var connectionstring;
var sastoken;
var fixedsastoken = false;

// ---------------------------------------------------------------
// validate settings from JSON file  passed as a parameter to the app
function validate_settings(settings, options) {
    console.log("Reading settings from config file");
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
// Force the SAS token
function set_sas_token(token)
{
    fixedsastoken = true;
    sastoken = token;
}

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

// ---------------------------------------------------------------
// Update the sas token
function update_sas_token()
{
    if (!fixedsastoken)
        sastoken = create_sas_token(connectionstring, settings.keyname, settings.key);
    console.log("New SAS token generated: " + sastoken);        
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
// Initializes connection settings to Event Hub
exports.init_connection = function(token)
{
    // Validate settings
    validate_settings(settings, ['namespace', 'keyname', 'key', 'eventhubname', 'displayname', 'guid', 'organization', 'location']);

    // ---------------------------------------------------------------
    // Get the full Event Hub publisher URI
    connectionstring = 'https://' + settings.namespace + '.servicebus.windows.net' + '/' + settings.eventhubname + '/publishers/' + settings.displayname + '/messages';
    console.log("Event Hub connection string: " + connectionstring);
 
    if (arguments.length >0)
    {
        fixedsastoken = true;
        sastoken = token;
    } else
    {
        // we need to recreate the sas token regularly as it has a limited validity in time
        update_sas_token();
        setInterval(function(){
            update_sas_token();
        }, 3600000);
    }
 };

// ---------------------------------------------------------------
// Send message to Event Hub
exports.send_message = function(measurename, unitofmeasure, value)
{
    var currentTime = new Date().toISOString();
    var message = format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, measurename, unitofmeasure, currentTime, value);
	console.log("Sending message: " + message);
    
    // Send the request to the Event Hub
    var http_options = {
            
        hostname: settings.namespace + '.servicebus.windows.net',
        port: 443,
        path: '/' + settings.eventhubname + '/publishers/' + settings.displayname + '/messages',
        method: 'POST',
        headers: {
            'Authorization': sastoken,
            'Content-Length': message.length,
            'Content-Type': 'application/atom+xml;type=entry;charset=utf-8'
        }
    };
        
    var req = https.request(http_options, function (res) {
        console.log("statusCode: ", res.statusCode);
        console.log("headers: ", res.headers);
            
        res.on('data', function (d) {
            console.log(d);
            if (res.statusCode == 401) update_sas_token();
        });
    });
        
    req.on('error', function (e) {
        console.error(e);
        // When we get an error, chances are our SAS token is not valid any more
        update_sas_token();
    });
        
    req.write(message);
        
    req.end();
}





