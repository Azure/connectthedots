<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="css/main.css" />

    <script type="text/javascript" src="http://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=7.0"></script>
    <script type="text/javascript" src="https://code.jquery.com/jquery-1.11.2.js"></script>
    <script type="text/javascript" src="js/jquery.ui.bmap.js"></script>
    <script type="text/javascript" src="js/jquery.ui.bmap.extensions.js"></script>
</head>
<body>
    <div id="top_header"></div>
    <div id="main_content">
        <div id="map_canvas"></div>
    </div>

    <script>
        $('#map_canvas').gmap({ 'credentials': 'Arkh4--7e-cy1ek_QH9oq3qerZut_jOPrR8h8C0Z69RKgE8aUuBe0inLrDWZvacw' }).bind('init', function (evt, map) {
            $('#map_canvas').gmap('getCurrentPosition', function (result, status) {
                if (status === 'OK') {
                    var clientPosition = new Microsoft.Maps.Location(result.position.coords.latitude, result.position.coords.longitude);
                    $('#map_canvas').gmap('addMarker', { 'location': clientPosition, 'bounds': true });
                }
            });
        });
    </script>
</body>
</html>
