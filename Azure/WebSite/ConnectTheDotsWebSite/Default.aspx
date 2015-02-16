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

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ConnectTheDotsWebSite.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Connect The Dots</title>

    <!-- general styles -->

    <link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/1.10.3/css/jquery.dataTables.css"/>
    <link rel="stylesheet" type="text/css" href="css/connectthedots.css"/>

    <script type="text/javascript" src="https://code.jquery.com/jquery-1.11.1.min.js"></script>
    <script type="text/javascript" src="https://cdn.datatables.net/1.10.3/js/jquery.dataTables.min.js"></script>
    <script src="http://d3js.org/d3.v3.min.js" charset="utf-8"></script>
    <script src="http://labratrevenge.com/d3-tip/javascripts/d3.tip.v0.6.3.js"></script>
    <script type="text/javascript" src="js/connectthedots.js"></script>
</head>
<body>

    <div id="loading">
        <div id="loading-inner">
            <p id="loading-text">Loading last 10 minutes of data...</p>
            <p id="loading-sensor"></p>
            <img id="loading-image" src="img/ajax-loader.gif" />
        </div>
    </div>

    <div id="header">
        <div>
            <img src="img/ConnectTheDotsLogo.png" />
        </div>
    </div>

    <form id="form2" runat="server">

        <div class="big-block">
            <h3>Live Sensor Data</h3>

            <div style="float: left; width: 200px">

                <p><strong>Select Sensor/R-PI:</strong></p>

                <ul id="sensorList">
                    <li class="selected">All</li>
                </ul>

            </div>


            <div style="margin-left: 200px;height: 600px;position:relative;text-align:center;">
                <div id="chartOne" class="chart">
                <h4>DataChart One</h4>
                    <script>
                        (function () {
                            registerChart("chartOne", ["1000", "1001"]);
                        })();
                    </script>
                </div>
                <div id="chartTwo" class="chart">
                <h4>DataChart Two</h4>
                    <script>

                        (function () {
                            registerChart("chartTwo", ["1002", "1003"]);
                        })();

                    </script>
                </div>
            </div>
        </div>

        <div class="big-block">
            <h3>Real Time Events</h3>
            <div id="alerts">
                <table id="alertTable">
                    <thead>
                        <tr>
                            <th class="timeFromDate">Time</th>
                            <th>Device</th>
                            <th>Alert</th>
                            <th>Message</th>
                        </tr>
                    </thead>
                    <tbody>
                    </tbody>
                </table>

            </div>
        </div>

        <div class="big-block">
            <h3>Messages</h3>
            <div id="messages"></div>
        </div>
    </form>
</body>
</html>
