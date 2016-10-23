function updateDevicesList() {
    // Get the devices list from the server
    PageMethods.GetDevicesList(Success, Failure);
}

function Success(result) {
    var devicesList = JSON.parse(result);
    var table = $('#devicesTable').DataTable();
    var addRow = true;

    for (var deviceIndex = 0 ; deviceIndex < devicesList.length; deviceIndex++) {
        var device = devicesList[deviceIndex];
        var location = 'unknown';
        if (device['location'] != null) location = device['location'];
        var ipaddress = 'unknown';
        if (device['ipaddress'] != null) ipaddress = device['ipaddress'];
        var displayname = 'unknown';
        if (device['displayname'] != null) displayname = device['displayname'];
        var connectionstring = 'unknown';
        if (device['connectionstring'] != null) connectionstring = device['connectionstring'];

        if (table.rows().length > 0) {
            // Check if we already have this one in the table already to prevent duplicates
            var indexes = table.rows().eq(0).filter(function (rowIdx) {
                if (
                    table.cell(rowIdx, 3).data() == device['guid']) {
                    // Update the row
                    table.cell(rowIdx, 0).innerHTML = displayname;
                    table.cell(rowIdx, 1).innerHTML = location;
                    table.cell(rowIdx, 2).innerHTML = ipaddress;
                    table.cell(rowIdx, 4).innerHTML = connectionstring;
                    return true;
                }
                return false;
            });
            // if the device is already in the list, return.
            if (indexes.length != 0 ) addRow = false;
        }

        // The device is a new one, lets add it to the table
        if (addRow == true) {
            table.row.add([
                displayname,
                location,
                ipaddress,
                device['guid'],
                connectionstring
            ]).draw();
        }
    }
}
function Failure(error) {
    alert(error);
}
