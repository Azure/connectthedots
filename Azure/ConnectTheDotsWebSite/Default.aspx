<!--
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
-->

<%@ page language="C#" inherits="WebClient.Default" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Default</title>
    
	<link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/1.10.3/css/jquery.dataTables.css">

    <script type="text/javascript" src="https://code.jquery.com/jquery-1.11.1.min.js"></script>
    <script type="text/javascript" src="https://code.highcharts.com/highcharts.js"></script>
    <script type="text/javascript" src="https://cdn.datatables.net/1.10.3/js/jquery.dataTables.min.js"></script>
    <script type="text/javascript" src="js/connectthedots.js"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div id="chartContainer" style="width: 100%; height: 400px;"></div>

    <div>
        <input type="button" onclick="ShowHide(alerts)" value="Show/Hide Alerts" />

        Select Sensor/R-PI:
        <asp:DropDownList ID="DropDownList1" runat="server" 
            DataSourceID="SensorInventoryData"
            onchange="SensorSelectionChanged(this)" > 
        </asp:DropDownList>
        <asp:ObjectDataSource ID="SensorInventoryData" runat="server" SelectMethod="GetSensorList" TypeName="WebClient.SensorInventory"></asp:ObjectDataSource>
    </div>


    <div id="alerts">
        <table id="alertTable" class="display">
            <thead>
                <tr>
                    <th>Alert Type</th>
                    <th class="timeFromDate" >Start</th>
                    <th class="timeFromDate" >End</th>
                    <th class="numberFixed">Temp Max</th>
                    <th class="numberFixed">Temp Average</th>
                    <th class="numberFixed">Temp Min</th>
                </tr>
            </thead>
            <tbody>
            </tbody>
        </table>

    </div>

    <input type="button" onclick="ShowHide(messages)" value="Show/Hide Live Data Table" />

    <div id="messages" style="display:none"></div>

    </form>
</body>
</html>

