// We are using the Cylon library to access the hardware resources, in particular the Weather Shield
var Cylon = require('cylon');

// Libraries for HTTP Rest connection to Azure event Hubs
//var https = require('https');
//var crypto = require('crypto');
//var moment = require('moment');

// Using AMQP for sending data to Azure Events Hub
var AMQPClient = require('./amqp_client');

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

var settingsFile = process.argv[2];
var settings = require('./' + settingsFile);
readSettings(settings, ['namespace', 'keyname', 'key', 'eventhubname', 'displayname', 'guid', 'organization', 'location']);

// Create AMQP connection string to connect to Azure Events Hub
var uri = 'amqps://' + encodeURIComponent(settings.keyname) + ':' + encodeURIComponent(settings.key) + '@' + settings.namespace + '.servicebus.windows.net';
var client = new AMQPClient(AMQPClient.policies.EventHubPolicy);

//// Full Event Hub publisher URI
//var my_uri = 'https://' + settings.namespace + '.servicebus.windows.net' + '/' + settings.eventHubName + '/publishers/' + settings.displayname + '/messages';

//// Create a SAS token
//// See http://msdn.microsoft.com/library/azure/dn170477.aspx
//function create_sas_token(uri, key_name, key)
//{
//    // Token expires in one hour
//    var expiry = moment().add(1, 'hours').unix();
//    var string_to_sign = encodeURIComponent(uri) + '\n' + expiry;
//    var hmac = crypto.createHmac('sha256', key);
//    hmac.update(string_to_sign);

//    var signature = hmac.digest('base64');

//    var token = 'SharedAccessSignature sr=' + encodeURIComponent(uri) + '&sig=' + encodeURIComponent(signature) + '&se=' + expiry + '&skn=' + key_name;

//    return token;

//}
//var my_sas = create_sas_token(my_uri, settings.SASKeyName, settings.SASKey);

//function send_message(payload, sas)
//{
//	console.log("Sending message: " + payload);
    
//    // Send the request to the Event Hub
//    var http_options = {
        
//        hostname: settings.namespace + '.servicebus.windows.net',
//        port: 443,
//        path: '/' + hubname + '/publishers/' + settings.displayName + '/messages',
//        method: 'POST',
//        headers: {
//            'Authorization': sas,
//            'Content-Length': payload.length,
//            'Content-Type': 'application/atom+xml;type=entry;charset=utf-8'
//        }
//    };
    
//    var req = https.request(http_options, function (res) {
//        console.log("statusCode: ", res.statusCode);
//        console.log("headers: ", res.headers);
        
//        res.on('data', function (d) {
//            process.stdout.write(d);
//        });
//    });
    
//    req.on('error', function (e) {
//        console.error(e);
//    });
    
//    req.write(payload);

//    req.end();
//}



var filterOffset = 43350; // example filter offset value might be: 43350; 
var filter;
if (filterOffset) {
    filter = { 
        'apache.org:selector-filter:string': AMQPClient.adapters.Translator( 
        ['described', ['symbol', 'apache.org:selector-filter:string'], ['string', "amqp.annotation.x-opt-offset > '" + filterOffset + "'"]]) 
    };
} 



function format_sensor_data(guid, displayname, organization, location, measurename, unitofmeasure, timecreated, value)
{
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

sendCB = function (tx_err, state) {
    if (tx_err) {
        console.log('Error Sending: ');
        console.log(tx_err);
        
    } else {
        console.log('Message sent');
    }
};

function send_message(message, time)
{
    console.log('Sending message with value ' + message);
    client.send(message, settings.eventhubname, { 'x-opt-partition-key': settings.guid }, sendCB);
}

//var currentTime = new Date().toISOString;

//function loop() {
//    setTimeout(function () {
//        currentTime = new Date().toISOString();
//        send_message(format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Temperature", "F", currentTime , 74), currentTime);
//        loop();
//    }, 1000);
//};

//client.connect(uri, function () {
//    loop();
//});

// Initialization of the Cylon object to get Weather shield data every second
Cylon.robot({
  connections: {
        edison: { adaptor: 'intel-iot' }//,
//        arduino: { adaptor: 'firmata', port: '/dev/ttyACM0' }
  },

  devices: {
        led: { driver: 'led', pin: 13 },
        shield_blue_led: { driver: 'led', pin: 7 },
        mpl115a2: { driver: 'mpl115a2' }
  },

  work: function(my) {
        every((1).second(), function () {
            my.led.toggle();
            my.shield_blue_led.toggle();
            
            var temp = 75;
            var pressure = 132;
            var currentTime = new Date().toISOString();
            var sendData = true;
            
            console.log("Reading temperature from sensors...");
            my.mpl115a2.getTemperature(function (err, data) {
                if (err) {
                    console.log("Error When reading temperature");
                    sendData = false;
                }
                else {
                    temp = data.temperature;
                    currentTime = new Date().toISOString();
                    sendData = true;
                }
            });
            if (sendData) {
                console.log(currentTime + " :Sending Temperature data " + temp);
                send_message(format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Temperature", "F", currentTime , temp), currentTime);
            }

            console.log("Reading pressure from sensors...");
            my.mpl115a2.getPressure(function (err, data) {
                if (err) {
                    console.log("Error When reading pressure");
                    sendData = false;
                }
                else {
                    pressure = data.pressure;
                    currentTime = new Date().toISOString();
                    sendData = true;
                }
            });
            if (sendData) {
                console.log(currentTime + " :Sending Pressure data " + pressure);
                send_message(format_sensor_data(settings.guid, settings.displayname, settings.organization, settings.location, "Pressure", "Pa", currentTime , pressure), currentTime);
            }
        });
    }
}).start();


