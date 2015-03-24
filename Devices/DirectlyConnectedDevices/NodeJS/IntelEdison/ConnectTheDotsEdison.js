// We are using the Cylon library to access the hardware resources, in particular the Weather Shield
var Cylon = require('cylon');

// Using HTTP Rest connection to Azure event Hubs
var https = require('https');
var crypto = require('crypto');
var moment = require('moment');
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

// Get the full Event Hub publisher URI
var my_uri = 'https://' + settings.namespace + '.servicebus.windows.net' + '/' + settings.eventhubname + '/publishers/' + settings.guid + '/messages';
    
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

// Initialization of the Cylon object to get Weather shield data every second
Cylon.robot( {
    connections: {
        edison: { adaptor: 'intel-iot' }
    },
    
    devices: {
        led: { driver: 'led', pin: 13 },
        shield_blue_led: { driver: 'led', pin: 7 },
        mpl115a2: { driver: 'mpl115a2' }
    },
    
    work: function (my) {
        // Regenerate the SAS token every hour
        every((3600000).second(), function () {
            my_sas = create_sas_token(my_uri, settings.keyname, settings.key);
        });
        
        // Every second, read sensor data and send to Event Hub
        every((1).second(), function () {
            my.led.toggle();
            my.shield_blue_led.toggle();
            
            var temp = 75;
            var pressure = 132;
            var currentTime = new Date().toISOString();
            
            console.log("Reading data from sensors...");
            my.mpl115a2.getPressure(function (err, data) {
                if (err) {
                    console.log("Error When reading sensor data");
                }
                else {
                    pressure = data.pressure;
                    temp = data.temperature;
                    currentTime = new Date().toISOString();
                    console.log(currentTime + " :Sending Temperature data " + temp);
                    send_message(format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Temperature", "F", currentTime , temp), currentTime);
                    console.log(currentTime + " :Sending Pressure data " + pressure);
                    send_message(format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Pressure", "Pa", currentTime , pressure), currentTime);
                }
            });

        });
    }
}).start();


