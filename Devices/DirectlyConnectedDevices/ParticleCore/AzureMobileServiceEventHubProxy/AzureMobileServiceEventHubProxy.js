var https = require('https');
var crypto = require('crypto');
var moment = require('moment');

exports.post = function(request, response) {
    sendTemperature(JSON.stringify(request.body));
    console.log(request.body);
    response.send(statusCodes.OK);
};

function sendTemperature(payload) {
// Event Hubs parameters
var namespace = 'EVENTHUBNAMESPACE';
var hubname ='EVENTHUBNAME';

// Shared access key (from Event Hub configuration) 
var my_key_name = 'KEYNAME'; 
var my_key = 'KEY';
    
// Payload to send
//payload = "{ \"temp\": \"100\", \"hmdt\": \"78\", \"subject\": \"wthr\", \"dspl\": \"test\"," + "\"time\": " + "\"" + new Date().toISOString() + "\" }";

// Full Event Hub publisher URI
var my_uri = 'https://' + namespace + '.servicebus.windows.net' + '/' + hubname  + '/messages';

// Create a SAS token
// See http://msdn.microsoft.com/library/azure/dn170477.aspx

function create_sas_token(uri, key_name, key)
{
    // Token expires in one hour
    var expiry = moment().add(1, 'hours').unix();

    var string_to_sign = encodeURIComponent(uri) + '\n' + expiry;
    var hmac = crypto.createHmac('sha256', key);
    hmac.update(string_to_sign);
    var signature = hmac.digest('base64');
    var token = 'SharedAccessSignature sr=' + encodeURIComponent(uri) + '&sig=' + encodeURIComponent(signature) + '&se=' + expiry + '&skn=' + key_name;

    return token;
}

var my_sas = create_sas_token(my_uri, my_key_name, my_key)

//console.log(my_sas);

// Send the request to the Event Hub

var options = {
  hostname: namespace + '.servicebus.windows.net',
  port: 443,
  path: '/' + hubname + '/messages',
  method: 'POST',
  headers: {
    'Authorization': my_sas,
    'Content-Length': payload.length,
    'Content-Type': 'application/atom+xml;type=entry;charset=utf-8'
  }
};

var req = https.request(options, function(res) {
  //console.log("statusCode: ", res.statusCode);
  //console.log("headers: ", res.headers);

  res.on('data', function(d) {
    //process.stdout.write(d);
  });
});

req.on('error', function(e) {
  //console.error(e);
});

req.write(payload);
req.end();

}