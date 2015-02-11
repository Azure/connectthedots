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

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <link rel="stylesheet" href="//code.jquery.com/ui/1.11.2/themes/ui-darkness/jquery-ui.css" />
    <link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/1.10.3/css/jquery.dataTables.css" />
    <link rel="stylesheet" href="/css/connectthedots.css" />
    <link rel="stylesheet" href="/css/kcs.css" />


    <script type="text/javascript" src="https://code.jquery.com/jquery-1.11.1.min.js"></script>
    <script src="//code.jquery.com/ui/1.11.2/jquery-ui.js"></script>




    <script type="text/javascript" src="https://cdn.datatables.net/1.10.3/js/jquery.dataTables.min.js"></script>
    <script src="http://d3js.org/d3.v3.min.js" charset="utf-8"></script>
    <script src="http://labratrevenge.com/d3-tip/javascripts/d3.tip.v0.6.3.js"></script>
    <script type="text/javascript" src="js/connectthedots.js"></script>

    <title>SLAC National Accelerator Laboratory</title>
</head>
<body>
    <div id="top_header" onclick="window.open( 'https://www6.slac.stanford.edu','_blank' ); return false;" />
    <div id="azure_logo" onclick="window.open( 'http://azure.microsoft.com/en-us/services/machine-learning','_blank' ); return false;"></div>
    <div id="otech_logo" onclick="window.open( 'https://msopentech.com','_blank' ); return false;"></div>
    <div id="main_menu">
        <ul id="main_menu_container"></ul>
    </div>
    </div>
    <div id="left">
        <div id="left_menu">
            <ul id="left_menu_container"></ul>
        </div>
    </div>
    <div id="center">
        <div class="pump_info" style="left: 0%">
            <img src="/img/pump.png" style="width: 100%; height: 100%;" />
            <div id="real_supply_temperature">
                <label class="header">Supply Temperature</label>
                <div class="left_data">
                    <label>PV:</label>
                    <label>SP:</label>
                    <label>OP:</label>
                    <label>Mode:</label>
                </div>
                <div class="center_data">
                    <label id="supply_temperature_current">&nbsp;</label>
                    <label id="supply_temperature_dest">&nbsp;</label>
                    <label id="supply_temperature_perc">&nbsp;</label>
                    <label id="supply_temperature_mode">&nbsp;</label>
                </div>
                <div class="right_data">
                    <label>°F</label>
                    <label>°F</label>
                    <label>%</label>
                </div>
            </div>
            <div id="real_heater">
                <label class="header">Heater</label>
                <div class="left_data">
                    <label>PV:</label>
                    <label>SP:</label>
                    <label>OP:</label>
                    <label>Mode:</label>
                </div>
                <div class="center_data">
                    <label id="heater_current">&nbsp;</label>
                    <label id="heater_dest">&nbsp;</label>
                    <label id="heater_perc">&nbsp;</label>
                    <label id="heater_mode">&nbsp;</label>
                </div>

            </div>
            <div id="real_pump">
                <label class="header">Pump</label>
                <div class="left_data" style="white-space: nowrap;">
                    <label>Return Temp:</label>
                    <label>Flow rate:</label>
                    <label>Pressure:</label>
                    <label>Conductivity:</label>
                </div>
                <div class="center_data" style="left: 45%;">
                    <label id="pump_temp">&nbsp;</label>
                    <label id="pump_flow">&nbsp;</label>
                    <label id="pump_pressure">&nbsp;</label>
                    <label id="pump_conductivity">&nbsp;</label>
                </div>
                <div class="right_data">
                    <label>°F</label>
                    <label>gpm</label>
                    <label>psi</label>
                    <label>uS</label>
                </div>

            </div>
            <div id="real_surge_tank">
                <label class="header">Surge Tank</label>
                <select id="real_surge_tank_1"></select>
                <select id="real_surge_tank_2"></select>
            </div>
        </div>
        <div class="pump_graph" style="left: 33%">
            <div id="loading">
                <div id="loading-inner">
                    <p id="loading-text">Loading last 10 minutes of data...</p>
                    <p id="loading-sensor"></p>
                    <img id="loading-image" src="img/ajax-loader.gif" />
                </div>
            </div>

            <div style="width: 100%; height: 100%;">
                <div id="Charts" style="margin-left: 0px">
                    <div id="Graphics">
                        <h4>Real Time Sensors Data</h4>
                        <div id="Temperature" class="chart">
                            <script>
                                (function () {
                                    chart("Temperature", "temperature (F)", 90, 98);  // make sure chart name matches the div id
                                })();
                            </script>
                        </div>

                        <div id="Flow_And_Pressure" class="chart">
                            <script>

                                (function () {
                                    chart("Flow_And_Pressure", "flow (gpm)", 400, 500, "pressure (psi)", 88, 95);  // make sure chart name matches the div id
                                })();

                            </script>
                        </div>
                    </div>
                    <div id="alerts">
                        <h4>Machine Learning Alerts</h4>
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
            </div>

            <div class="big-block" style="display: none;">

                <h3>Messages</h3>

                <div id="messages"></div>
            </div>

        </div>
    </div>
    <div id="bottom">
        <input type="button" id="generate_anomalies_btn" onclick="onGenerateAnomalies()" value="Generate Anomalies" />
        <div id="ctd_logo" onclick="window.open( 'http://connectthedots.io','_blank' ); return false;"></div>
        <label id="designed_label">Designed with</label>
    </div>
    <script>
        // predefined values
        var predefined_SP = 95.0;
        var predefined_cond = 0.12;
        var predefined_mode = 'Auto';
        var surgeTankOptions1 = ['select ..'];
        var surgeTankOptions2 = ['select ..'];

        var topMenu = ['FACET Overview', 'FACET Injector', 'LI-00', 'LI-01', 'LI-02', 'LI-03', 'LI-04', 'LI-05', 'LI-06', 'LI-07', 'LI-08', 'LI-09', 'LI-10', 'LI-11', 'LI-12', 'LI-13'];
        var leftMenu = ['Overview', 'LCLS', 'LCLS II', 'FACET'];

        function menuFromArray(array, container) {
            $.each(array, function (i, item) {
                $('<li>').attr('padding', '5px 0px').html(
                $('<a>').attr({ 'href': 'javascript: void();' }).text(item)).appendTo(container);
            });
        }

        function onResize() {
            //Standard height, for which the body font size is correct
            var preferredHeight = 768;
            //Base font size for the page
            var fontsize = 12;

            var displayHeight = $(window).height();
            var percentage = displayHeight / preferredHeight;
            var newFontSize = Math.floor(fontsize * percentage) - 1;
            $("#real_supply_temperature").css("font-size", newFontSize + 'px');
            $("#real_heater").css("font-size", newFontSize + 'px');
            $("#real_pump").css("font-size", newFontSize + 'px');
            $("#real_surge_tank").css("font-size", newFontSize + 'px');
            $("#real_surge_tank_1").css("font-size", newFontSize + 'px');
            $("#real_surge_tank_2").css("font-size", newFontSize + 'px');
        }

        $(function () {
            // fill predefined data
            $("#supply_temperature_dest").text(predefined_SP);
            $("#supply_temperature_mode").text(predefined_mode);
            $("#heater_mode").text(predefined_mode);
            $("#pump_conductivity").text(predefined_cond);

            $.each(surgeTankOptions1, function (i, item) {
                $('<option>').text(item).appendTo($("#real_surge_tank_1"));
            });

            $.each(surgeTankOptions2, function (i, item) {
                $('<option>').text(item).appendTo($("#real_surge_tank_2"));
            });

            // create main menu
            menuFromArray(topMenu, '#main_menu_container');
            $("#main_menu").tabs();

            // create left menu
            menuFromArray(leftMenu, '#left_menu_container');
            $("#left_menu").tabs().addClass("ui-tabs-vertical ui-helper-clearfix");
            $("#left_menu li").removeClass("ui-corner-top").addClass("ui-corner-left");

            // resize listerner
            $(window).bind('resize', function () {
                onResize();
            }).trigger('resize');
        });
    </script>
</body>
</html>
