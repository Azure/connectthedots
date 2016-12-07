function updateDevicesList() {
    // Get the devices list from the server
    PageMethods.GetDevicesList(ListSuccess, Failure);
}

var addDeviceDialog, addDeviceForm, confirmDeleteDeviceDialog;

function addNewDevice()
{
    var newDeviceID = $("#newdeviceid").val();
    addDeviceDialog.dialog("close");
    PageMethods.AddDevice(newDeviceID, AddSuccess, Failure);
}

function deleteDevice(deviceID)
{
    var id = deviceID;
    confirmDeleteDeviceDialog.dialog("close");
    PageMethods.DeleteDevice(id, DeleteSuccess, Failure);
}

addDeviceDialog = $("#add-device-dialog-form").dialog({
    autoOpen: false,
    height: "auto",
    width: 400,
    modal: true,
    buttons: {
        "Ok": addNewDevice,
        Cancel: function () {
            addDeviceDialog.dialog("close");
        }
    },
    close: function () {
        addDeviceForm[0].reset();
    }
});

confirmDeleteDeviceDialog = $("#delete-device-dialog-confirm").dialog({
    autoOpen: false,
    resizable: false,
    height: "auto",
    width: 400,
    modal: true,
    buttons: {
        "Delete device": function () {
            deleteDevice(confirmDeleteDeviceDialog.data('deviceID'));
        },
        Cancel: function () {
            confirmDeleteDeviceDialog.dialog("close");
        }
    }
});


addDeviceForm = addDeviceDialog.find("form").on("submit", function (event) {
    event.preventDefault();
    addNewDevice();
});

//function addDevice()
//{
//    var deviceName = prompt("Enter a unique Device Id");
//    if (deviceName)
//        PageMethods.AddDevice(deviceName, AddSuccess, Failure);
//}

//function deleteDevice() {
//    var deviceName = prompt("Enter the IoT Hub ID of the device you want to remove");
//    if (deviceName)
//        PageMethods.DeleteDevice(deviceName, DeleteSuccess, Failure);
//}

function ListSuccess(result) {
    if (result) {
        var devicesList = JSON.parse(result);
        var table = $('#devicesTable').DataTable();

        // Check if we need to remove a row from the table
        var rowsToRemove=[];
        for (var rowIndex = 0; rowIndex < table.rows().eq(0).length; rowIndex++) {
            for (var deviceIdx = 0 ; deviceIdx < devicesList.length ; deviceIdx++)
            {
                if (devicesList[deviceIdx]['guid'] == table.cell(rowIndex, 3).data()) {
                    rowsToRemove[rowsToRemove.length] = rowIndex;
                    break;
                }
            }
        }
        for (var idx = rowsToRemove.length; idx > 0 ; idx--)
        {
            table.rows(idx).remove().draw();
        }

        // Check if we need to update or add a row in the table
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

            var addRow = true;

            if (table.rows().length > 0) {
                // Check if we already have this one in the table already to prevent duplicates
                var indexes = table.rows().eq(0).filter(function (rowIdx) {
                    if (
                        table.cell(rowIdx, 3).data() == device['guid']) {
                        // Update the row
                        table.cell(rowIdx, 0).innerHTML = displayname;
                        table.cell(rowIdx, 1).innerHTML = location;
                        table.cell(rowIdx, 2).innerHTML = ipaddress;
                        if ($('#cscolumn').is(':visible')) {
                            table.cell(rowIdx, 4).innerHTML = connectionstring;
                        }
                        return true;
                    }
                    return false;
                });
                // if the device is already in the list, return.
                if (indexes.length != 0) addRow = false;
            }

            // The device is a new one, lets add it to the table
            if (addRow == true) {
                if ($('#cscolumn').is(':visible')) {
                    table.row.add([
                        displayname,
                        location,
                        ipaddress,
                        device['guid'],
                        connectionstring
                    ]).draw();
                } else {
                    table.row.add([
                        displayname,
                        location,
                        ipaddress,
                        device['guid']
                    ]).draw();
                }
            }
        }
    }
}

function AddSuccess(result) {
    if (result) {
        var resultObject = JSON.parse(result);
        if (resultObject.Error) 
        {
            addOutputToConsole('ERROR ' + resultObject.Error);
            alert(resultObject.Error);
        } else {
            addOutputToConsole('Device ' + resultObject.Device + ' added to IoT Hub');
            updateDevicesList();
        }
    } else {
        addOutputToConsole('An error happened while trying to add a new device');
        alert("An error happened while trying to add a new device");
    }
}

function DeleteSuccess(result) {
    if (result) {
        var resultObject = JSON.parse(result);
        if (resultObject.Error) {
            addOutputToConsole('ERROR ' + resultObject.Error);
            alert(resultObject.Error);
        } else {
            addOutputToConsole('Device ' + resultObject.Device + ' deleted from IoT Hub');
            updateDevicesList();
        }
    } else {
        addOutputToConsole('An error happened while trying to delete the device');
        alert('An error happened while trying to delete the device');
    }
}

function Failure(error) {
    addOutputToConsole(error);
    alert(error);
}

