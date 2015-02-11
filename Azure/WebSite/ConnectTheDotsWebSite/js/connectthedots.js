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
//var updateChart = true;
var websocket;
var isBulking = false;
var receivedFirstMessage = false;

// constants

var MAX_ARRAY_SIZE = 1000;

var MS_MIN_INTERVAL = 100;
var MS_PER_MINUTE = 60000;
var WINDOW_MINUTES = 10;

var CHART_1_NAME = "Temperature";
var CHART_2_NAME = "Flow_And_Pressure";

// globals

var D3_chart1 = [];
var D3_chart2 = [];

// keep track of absolute freshest sample point

var freshestTime = [];
freshestTime[CHART_1_NAME] = null;
freshestTime[CHART_2_NAME] = null;

// globals used with D3

var path = {};
var legend = {};
var legend_r = {};
var x = null;
var y0 = null;
var y1 = null;
var height = {};
var width = {};
var svg = {};
var tip = {};
var color = null;
var sensorNames = [];

// initialize color

//sensorNames.push("avg")
color = d3.scale.category10();
color.domain(sensorNames);

// worker-related

var sss = null;

// deep copy of any object
function deepCopy(obj) {
    if (typeof obj != 'object') {
        return obj;
    }
    var copy = obj.constructor();
    for (var key in obj) {
        if (typeof obj[key] == 'object' && obj[key] != null) {
            copy[key] = this.deepCopy(obj[key]);
        } else {
            copy[key] = obj[key];
        }
    }
    return copy;
};


// Create the chart(s) that will be used to
// display the live data.  We use the D3.js
// library to establish SVG elements that will
// do the rendering

function chart(chart_name, y0_label, y0_min, y0_max, y1_label, y1_min, y1_max) {

    var margin = {
        top: 30,
        right: 250,
        bottom: 20,
        left: 50
    };

    var container = $('#' + chart_name);

    //Standard height, for which the body font size is correct
    var preferredHeight = 768;
    //Base font size for the page
    var fontsize = 12;

    var displayHeight = $(window).height();
    var percentage = displayHeight / preferredHeight;
    var newFontSize = Math.floor(fontsize * percentage) - 1;

    width = container.width() - margin.right;
    height = container.height() - margin.top - margin.bottom;

    // seed the axes with some dummy values

    x = d3.time.scale()
		.domain([new Date("2015-01-01T04:02:39.867841Z"), new Date("2015-01-01T04:07:39.867841Z")])
		.range([0, width]);

    y0 = d3.scale.linear()
		.domain([y0_min, y0_max])
		.range([height, 0]);

    y1 = d3.scale.linear()
		.domain([y1_min, y1_max])
		.range([height, 0]);

    svg[chart_name] = d3.select("#" + chart_name)
		.append("p")
		.append("svg")
		.attr("width", width + margin.left + margin.right)
		.attr("height", height + margin.top + margin.bottom)
		.style("margin-left", margin.left + "px")
		.style("margin-bottom", margin.bottom + "px")
		.style("margin-right", margin.right + "px")
		.append("g")
		.attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    svg[chart_name].append("g")
		.attr("class", "y0 axis")
		.call(d3.svg.axis().scale(y0).ticks(7).orient("left"));

    svg[chart_name].append("text")
		.attr("transform", "rotate(-90)")
		.attr("class", "y0 label")
		.attr("text-anchor", "middle")
		.attr("y", -50)
		.attr("x", -height / 2)
		.attr("dy", "1em")
		.attr("font-size", newFontSize + "px")
		.text(y0_label);


    if (y1_label) {
        svg[chart_name].append("text")
			.attr("y1", 0 - (margin.top / 2))

        svg[chart_name].append("g")
			.attr("class", "y1 axis")
			.attr("transform", "translate(" + width + ",0)")
			.text(y1_label)
			.call(d3.svg.axis().scale(y1).ticks(7).orient("right"));

        svg[chart_name].append("text")
			.attr("transform", "rotate(-90)")
			.attr("class", "y1 label")
			.attr("text-anchor", "middle")
			.attr("y", width + 30)
			.attr("x", -height / 2)
			.attr("dy", "1em")
			.attr("font-size", newFontSize + "px")
			.text(y1_label);
    }

    svg[chart_name].append("g")
		.attr("class", "x axis")
		.attr("transform", "translate(0," + (height) + ")")
		.call(d3.svg.axis().scale(x).orient("bottom").tickFormat(d3.time.format("%X")));

    path[chart_name] = {};
    legend[chart_name] = {};
    legend_r[chart_name] = {};

    // create tip
    tip[chart_name] = d3.tip()
        .attr('class', 'd3-tip')
        .offset([-10, 0])
        .html(function (d) {
            return "<label class='time_header'>" + d.time + "</label><label class='value_circle'>&#x25cf;</label><label class='value'>" + d.data.toFixed(2) + "</label><label class='message'>" + d.alertData.message + "</label>";
        });
    svg[chart_name].call(tip[chart_name]);

    if (!window['resizeCallback@' + chart_name]) {
        window['resizeCallback@' + chart_name] = true;
        $(window).bind('resize', function () {
            console.log('rezise chart: ' + chart_name);
            // remove original one
            d3.select("#" + chart_name).select('svg').remove();
            // clean up alert points

            // create a new one w/ correct size
            chart(chart_name, y0_label, y0_min, y0_max, y1_label, y1_min, y1_max);
        }).trigger('resize');
    }
}

//
// MakeNewD3Series
//
// create a new D3 data series
//
//  each object in the array looks like this:
//
//  {
//      name: "<series name>"
//      data: [{
//                  datapoint (float)
//                  timestamp (Date object)
//            }],...N-1
//  }
//

function MakeNewD3Series(d3_data, series_name) {

    var obj = {
        name: series_name,
        data: []
    };
    d3_data.push(obj);

    return obj;
}

//
// InsertNewDatapoint
//
// Push a new datapoint onto the queue.
// We want to keep the queue filled with WINDOW_MINUTES
// amount of data.  But we always keep it
// capped at MAX_ARRAY_SIZE number of items
//
// The datapoints come in out of order during
// bulk operations.  This necessitates a sorted
// insertion...which is NOT what we want here.
// Is there a way to get the bulk data to arrive
// sorted ??
//

function InsertNewDatapoint(data, time, val, y_axis, alertData) {
    var t = new Date(time);
    if (isNaN(t.getTime())) {
        //console.log("invalid date");
        return;
    }

    var now = new Date();
    var cutoff = new Date(now - WINDOW_MINUTES * MS_PER_MINUTE)

    if (t < cutoff) {
        //console.log("too old");
        return;
    }

    var pushObj = {
        data: val,
        time: new Date(time),
        y_axis: y_axis,
    };

    if (alertData)
        pushObj.alertData = alertData;

    data.push(pushObj);

    if (data.length >= MAX_ARRAY_SIZE) { //should never be greater, but...
        data.shift();
        return;
    }
}

//
// AddToD3
//
// Add a new datapoint to the appropriate
// dataset.
//

function AddToD3(D3_set, chart_name, series_name, val, time, y_axis, alertData) {

    var data = null;
    for (var i = 0; i < D3_set.length; i++) {
        if (D3_set[i].name == series_name) {
            data = D3_set[i].data;
            break;
        }
    }

    if (data == null) {
        data = MakeNewD3Series(D3_set, series_name).data;
    }

    // insert the new datapoint into the dataset

    InsertNewDatapoint(data, time, val, y_axis, alertData);
}

//
// PruneOld3DData
//
//  Removes any datapoints that are older than 10 minutes.
//  If any datasets are completely empty afterwards, clear
//  that sensor entirely from the list.  This function is
//  needed to clear sensors that have gone offline and are
//  not producing any more data.
//

function PruneOldD3Data(D3_set, chart_name) {
    var now = new Date();
    var cutoff = new Date(now - WINDOW_MINUTES * MS_PER_MINUTE)

    console.log(now);
    console.log(cutoff);

    for (var i = 0; i < D3_set.length; i++) {
        var data = D3_set[i].data;

        //for (var j = data.length - 1; j >= 0; j --){
        //    if (data[j].time < cutoff){
        //        data.splice(j,1);
        //    }
        //}

        while (data.length >= 1 && data[0].time < cutoff) {
            data.shift();
        }
    }

    var idxToRemove = [];

    for (var i = 0; i < D3_set.length; i++) {

        if (D3_set[i].data.length == 0) {

            if (path[chart_name] == null) {
                continue;
            }

            if (path[chart_name][D3_set[i].name] != null) {
                path[chart_name][D3_set[i].name].remove();
                path[chart_name][D3_set[i].name] = null;
            }

            if (legend[chart_name][D3_set[i].name] != null) {
                legend[chart_name][D3_set[i].name].remove();
                legend[chart_name][D3_set[i].name] = null;
            }

            if (legend_r[chart_name][D3_set[i].name] != null) {
                legend_r[chart_name][D3_set[i].name].remove();
                legend_r[chart_name][D3_set[i].name] = null;
            }

            idxToRemove.push(i);
        }
    }

    for (var i = 0; i < idxToRemove.length; i++) {
        console.log("splicing");
        D3_set.splice(idxToRemove[i], 1);
    }
}

//
// UpdateD3Charts
//
//  Refreshes D3 charts with new data.  Rebinds data
//  arrays, recomputes ranges for x and y axes
//

function UpdateD3Charts(D3_set, chart_name) {

    var minDate = new Date("3015-01-01T04:02:39.867841Z");
    var maxDate = new Date("1915-01-01T04:02:39.867841Z")

    var minVal = [Number.MAX_VALUE, Number.MAX_VALUE];
    var maxVal = [0, 0];

    var displayHeight = $(window).height();

    for (var i = 0; i < D3_set.length; i++) {

        if (sensorNames.indexOf(D3_set[i].name) == -1) {
            sensorNames.push(D3_set[i].name);
        }

        var data = D3_set[i].data;
        var y = data[0].y_axis;

        for (var j = 0; j < data.length; j++) {

            var c = data[j].data;
            var t = data[j].time;

            if (c < minVal[y]) {
                minVal[y] = c;
            }

            if (c > maxVal[y]) {
                maxVal[y] = c;
            }

            if (t > maxDate) {
                maxDate = t;
            }

            if (t < minDate) {
                minDate = t;
            }
        }
    }

    color.domain(sensorNames);

    if (minVal[0] < Number.MAX_VALUE) {
        var scaleMargin = (maxVal[0] - minVal[0]) * 10 / 100;
        y0 = d3.scale.linear()
			.domain([minVal[0] - scaleMargin, maxVal[0] + scaleMargin])
			.range([height, 0]);

        var yAxisLeft = d3.svg.axis()
			.scale(y0)
			.orient("left")

        svg[chart_name].selectAll("g.y0.axis")
			.call(yAxisLeft);
    }

    if (minVal[1] < Number.MAX_VALUE) {
        var scaleMargin = (maxVal[1] - minVal[1]) * 10 / 100;

        y1 = d3.scale.linear()
			.domain([minVal[1] - scaleMargin, maxVal[1] + scaleMargin])
			.range([height, 0]);

        var yAxisRight = d3.svg.axis()
			.scale(y1)
			.orient("right")

        svg[chart_name].selectAll("g.y1.axis")
			.call(yAxisRight);
    }

    x = d3.time.scale()
		.domain([minDate, maxDate])
		.range([0, width]);

    var xAxis = d3.svg.axis()
		.scale(x)
		.tickFormat(d3.time.format("%X"))
		.orient("bottom");

    svg[chart_name].selectAll("g.x.axis")
		.call(xAxis);

    var line = [
		d3.svg.line()
        .interpolate("monotone")
		.x(function (d) {
		    return x(d.time);
		})
		.y(function (d) {
		    return y0(d.data);
		}),

		d3.svg.line()
        .interpolate("monotone")
		.x(function (d) {
		    return x(d.time);
		})
		.y(function (d) {
		    return y1(d.data);
		})
    ];

    for (var i = 0; i < D3_set.length; i++) {

        try {

            var data = D3_set[i].data;
            var name = D3_set[i].name;

            // sort data
            data.sort(function (a, b) {
                if (a.time < b.time) return -1;
                if (a.time > b.time) return 1;
                return 0;
            });

            if (path[chart_name][name] == null) {
                path[chart_name][name] = svg[chart_name].append("g")
					.append("path")
					.datum(data)
					.attr("class", "line")
					.attr("d", line[data[0].y_axis])
					.style("stroke", function (d) {
					    return color(name);
					});
            }

            path[chart_name][name]
                .datum(data)
                .attr("d", line[data[0].y_axis]);

            // draw alert points
            for (var pnt in data) {
                if (typeof data[pnt].alertData == 'object') {
                    if (data[pnt].alertShowed == undefined) {
                        var transferData = JSON.stringify({ alertData: data[pnt].alertData, time: data[pnt].time, data: data[pnt].data });

                        data[pnt].alertBarShowed = svg[chart_name].append("g").append("rect")
                            .attr("class", "bar")
							.attr("x", x(data[pnt].time))
							.attr("y", 0)
                            .attr("height", height)
							.attr("width", "2px")
                            .style("fill", "#e6c9cd")

                        data[pnt].alertShowed = svg[chart_name].append("g").append("circle")
                            .attr("class", "d3-dot")
							.attr("cx", x(data[pnt].time))
							.attr("cy", data[pnt].y_axis == 0 ? y0(data[pnt].data) : y1(data[pnt].data))
							.style("fill", "#e93541")
							.attr("r", displayHeight / 200)
                            .on('mouseover', function () { d3.select(this).transition().attr("r", displayHeight / 130); eval("tip[chart_name].show(" + transferData + ");") })
                            .on('mouseout', function () { d3.select(this).transition().attr("r", displayHeight / 200); tip[chart_name].hide(); });
                    } else {
                        data[pnt].alertShowed.attr("cx", x(data[pnt].time))
							.attr("cy", data[pnt].y_axis == 0 ? y0(data[pnt].data) : y1(data[pnt].data));

                        data[pnt].alertBarShowed
                            .attr("x", x(data[pnt].time))
                    }
                }
            }

            if (legend[chart_name][name] == null) {
                legend_r[chart_name][name] = svg[chart_name].append("rect")
					.attr("class", "legend")
					.attr("width", 10)
					.attr("height", 10)
					.attr("x", width + 50)
					.attr("y", 20 + (20 * i))
					.style("fill", color(name))
					.style("stroke", color(name));

                var legend_display_name = name;
                if (name.toLowerCase().indexOf("return_temp") != -1) legend_display_name = "Return Temp";
                else if (name.toLowerCase().indexOf("supply_temp") != -1) legend_display_name = "Supply Temp";
                else if (name.toLowerCase().indexOf("pressure") != -1) legend_display_name = "Pressure";
                else if (name.toLowerCase().indexOf("flow") != -1) legend_display_name = "Flow";

                legend[chart_name][name] = svg[chart_name].append("text")
					.attr("x", width + 65)
					.attr("y", 20 + (20 * i) + 5)
					.attr("class", "legend")
					.style("fill", color(name))
					.text(legend_display_name == 'avg' ? 'avg (of all sensors)' : legend_display_name);

            }
        } catch (e) {
            console.log(e);
        }
    }
}


function RemoveFromChart(chart_name, series_name) {

    path[chart_name][series_name].remove();
    path[chart_name][series_name] = null;

    legend[chart_name][series_name].remove();
    legend[chart_name][series_name] = null;

    legend_r[chart_name][series_name].remove();
    legend_r[chart_name][series_name] = null;
}

function ResetChart(chart_name) {
    path[chart_name] = {};
    legend[chart_name] = {};
    legend_r[chart_name] = {};
}

//
// ClearD3Charts
//
//  Removes all graphical elements from the SVG objects
//  representing the charts.  Clears all arrays holding
//  chart data
//

function ClearD3Charts() {

    for (var i = 0; i < D3_chart1.length; i++) {
        RemoveFromChart(CHART_1_NAME, D3_chart1[i].name);
    }

    ResetChart(CHART_1_NAME);

    for (var i = 0; i < D3_chart2.length; i++) {
        RemoveFromChart(CHART_2_NAME, D3_chart2[i].name);
    }

    ResetChart(CHART_2_NAME);

    D3_chart1 = [];
    D3_chart2 = [];
}

//
// JQuery ready function
//

$(document).ready(function () {

    $('#loading').hide();

    //
    // Set up jQuery DataTable to show alerts
    //

    var table = $('#alertTable').DataTable({
        "bAutoWidth": false,
        "bFilter": false,
        "bInfo": false,
        "paging": false,
        "order": [
			[0, "desc"]
        ],
        "columnDefs": [{
            "targets": "timeFromDate",
            "data": function (row, type, val, meta) {
                if (type === 'set') {
                    row[meta.col] = val;
                    return;
                } else if (type === 'display') {
                    return row[meta.col].toLocaleTimeString();
                }
                return row[meta.col];
            }
        }, {
            "targets": "numberFixed",
            "data": function (row, type, val, meta) {
                if (type === 'set') {
                    row[meta.col] = val;
                    return;
                } else if (type === 'display') {
                    return row[meta.col].toFixed(1);
                }
                return row[meta.col];
            }
        }, ]
    });

    // Set up websocket client

    var sss = (window.location.protocol.indexOf('s') > 0 ? "s" : "");

    var uri = 'ws' + sss + '://' + window.location.host + '/api/websocketconnect?clientId=none';

    //var uri = 'ws' + sss + '://' + 'connectthedots.msopentech.com' + '/api/websocketconnect?clientId=none';

    websocket = new WebSocket(uri);

    $('#messages').prepend('<div> Connecting to ' + uri + '<div>');

    websocket.onopen = function () {
        $('#messages').prepend('<div>Connected.</div>');
    }

    websocket.onerror = function (event) {
        console.log(event);
        $('#messages').prepend('<div>ERROR ' + event.error + '</div>');
    }

    // Deal with message received on WebSocket
    websocket.onmessage = function (event) {
        try {
            // Parse the JSON package
            var eventObject = JSON.parse(event.data);
        } catch (e) {
            $('#messages').prepend('<div>Malformed message: ' + event.data + "</div>");
        }

        // update pump data
        onUpdatePump(eventObject);

        // initialize the page with all sensors

        if (receivedFirstMessage == false) {

            ClearD3Charts();
            var x = {
                MessageType: "LiveDataSelection",
                DeviceName: 'All'
            };

            websocket.send(JSON.stringify(x));
            receivedFirstMessage = true;
        }

        // Seems like we have valid data
        try {

            if (eventObject.dspl != null && eventObject.average == null && eventObject.dataSourceName != undefined) {

                // if we have a new sensor, add it to the list

                var sensorName = eventObject.dspl + ":" + eventObject.dataSourceName;
                var exists = (sensorNames.indexOf(sensorName) != -1);

                if (exists == false) {
                    sensorNames.push(sensorName);
                }
            }
            var chart = undefined;
            var chartName = undefined;

            // If the message is an alert, we need to display it in the datatable
            if (eventObject.alerttype != null && isBulking == false) {
                var table = $('#alertTable').DataTable();
                var time = new Date(eventObject.timestart);

                // Log the alert in the rawalerts div
                $('#rawalerts').prepend('<div>' + time + ': ' + eventObject.dsplalert + ' ' + eventObject.alerttype + ' ' + eventObject.message + '</div>');
                $('#rawalerts').contents().filter(':gt(20)').remove();

                // Check if we already have this one in the table already to prevent duplicates
                var indexes = table.rows().eq(0).filter(function (rowIdx) {
                    if (
						table.cell(rowIdx, 0).data().getTime() == time.getTime() && table.cell(rowIdx, 1).data() == eventObject.dsplalert && table.cell(rowIdx, 2).data() == eventObject.alerttype) {
                        return true;
                    }
                    return false;
                });

                // The alert is a new one, lets display it
                if (indexes.length == 0) {
                    // For performance reasons, we want to limit the number of items in the table to a max of 20. 
                    // We will remove the oldest from the list
                    if (table.data().length > 19) {
                        // Search for the oldest time in the list of alerts
                        var minTime = table.data().sort(

							function (a, b) {
							    return (a[0] > b[0]) - (a[0] < b[0])
							})[0][0];
                        // Delete the oldest row
                        table.rows(

							function (idx, data, node) {
							    return data[0].getTime() == minTime.getTime();
							}).remove();
                    }

                    // Add the new alert to the table
                    var message = 'message';
                    if (eventObject.message != null) message = eventObject.message;
                    table.row.add([
						time,
						eventObject.dsplalert,
						eventObject.alerttype,
						message
                    ]).draw();


                    var alertData = { message: message };

                    // add to charts
                    if (eventObject.average != null) {
                        AddToD3(D3_chart1, CHART_1_NAME, "avg", eventObject.valueavg, eventObject.timestart, 0, alertData);
                        chart = D3_chart1;
                        chartName = CHART_1_NAME;
                    } else {
                        var sensorName = eventObject.dspl + ":" + eventObject.dataSourceName;

                        if (eventObject.dataSourceName.toLowerCase().indexOf("temp") != -1) {
                            AddToD3(D3_chart1, CHART_1_NAME, sensorName, eventObject.value, eventObject.timestart, 0, alertData);
                            chart = D3_chart1;
                            chartName = CHART_1_NAME;
                        } else if (eventObject.dataSourceName.toLowerCase().indexOf("flow") != -1) {
                            AddToD3(D3_chart2, CHART_2_NAME, sensorName, eventObject.value, eventObject.timestart, 0, alertData);
                            chart = D3_chart2;
                            chartName = CHART_2_NAME;
                        } else if (eventObject.dataSourceName.toLowerCase().indexOf("pressure") != -1) {
                            AddToD3(D3_chart2, CHART_2_NAME, sensorName, eventObject.value, eventObject.timestart, 1, alertData);
                            chart = D3_chart2;
                            chartName = CHART_2_NAME;
                        }
                    }

                }
            } else {
                // Message received is not an alert. let's display it in the charts

                if (eventObject.average != null) {

                    if (eventObject.time != null) {
                        AddToD3(D3_chart1, CHART_1_NAME, "avg", eventObject.valueavg, eventObject.time, 0);
                        chart = D3_chart1;
                        chartName = CHART_1_NAME;
                    }
                    if (isBulking) {
                        $('#loading-sensor').text("avg");
                    }
                } else if (eventObject.bulkData != null) {

                    // Don't update while receiving bulk data.  It will
                    // cause the browser to (usually) freeze

                    if (eventObject.bulkData == true) {
                        $('#loading').show();
                        isBulking = true;
                        ClearD3Charts();
                    } else {

                        PruneOldD3Data(D3_chart1, CHART_1_NAME);
                        PruneOldD3Data(D3_chart2, CHART_2_NAME);

                        UpdateD3Charts(D3_chart1, CHART_1_NAME);
                        UpdateD3Charts(D3_chart2, CHART_2_NAME);

                        $('#loading').hide();

                        isBulking = false;
                    }
                } else {
                    // the message is data for the charts
                    // we make sure the dspl field is in the message, meaning the data is coming from a known device or service
                    if (eventObject.dspl != null) {
                        var sensorName = eventObject.dspl + ":" + eventObject.dataSourceName;

                        if (eventObject.time != null) {
                            if (eventObject.dataSourceName.toLowerCase().indexOf("temp") != -1) {
                                AddToD3(D3_chart1, CHART_1_NAME, sensorName, eventObject.value, eventObject.time, 0);
                                chart = D3_chart1;
                                chartName = CHART_1_NAME;
                            } else if (eventObject.dataSourceName.toLowerCase().indexOf("flow") != -1) {
                                AddToD3(D3_chart2, CHART_2_NAME, sensorName, eventObject.value, eventObject.time, 0);
                                chart = D3_chart2;
                                chartName = CHART_2_NAME;
                            } else if (eventObject.dataSourceName.toLowerCase().indexOf("pressure") != -1) {
                                AddToD3(D3_chart2, CHART_2_NAME, sensorName, eventObject.value, eventObject.time, 1);
                                chart = D3_chart2;
                                chartName = CHART_2_NAME;
                            }
                        }

                    }
                }
            }
            if (!isBulking && chart && chartName) {
                PruneOldD3Data(chart, chartName);
                UpdateD3Charts(chart, chartName);
            } else {
                $('#loading-sensor').text(sensorName);
            }

        } catch (e) {

            $('#messages').prepend('<div>Error processing message: ' + e.message + "</div>");
        }
    }

});

function onUpdatePump(eventObject) {
    if (!eventObject || typeof eventObject != 'object' || eventObject.value == undefined) return;

    var value = eventObject.value.toFixed(2);
    switch (eventObject.dataSourceName) {
        case 'ML/Sector_11_KCS_Return_Temp':
            $("#pump_temp").text(value);
            break;
        case 'ML/Sector_11_KCS_Flow':
            $("#pump_flow").text(value);
            break;
        case 'ML/Sector_11_KCS_Pressure':
            $("#pump_pressure").text(value);
            break;
        case "ML/Sector_11_KCS_Supply_Temp":
            $("#supply_temperature_current").text(value);
            var op = parseFloat($("#supply_temperature_dest").text()) * 100 / parseFloat(value);
            $("#supply_temperature_perc").text(op.toFixed(2));

            break;
    }
}

function SensorSelectionChanged(dropDown) {
    var newSensor = dropDown.value;
    if (websocket != null) {

        ClearD3Charts();

        var x = {
            MessageType: "LiveDataSelection",
            DeviceName: newSensor
        };
        websocket.send(JSON.stringify(x));
    }

}

function onGenerateAnomalies() {
    var btn = $("#generate_anomalies_btn");
    var generate = false;

    if (btn[0].value == "Generate Anomalies") {
        btn[0].value = "Stop Generate Anomalies";
        generate = true;
    }
    else {
        btn[0].value = "Generate Anomalies";
    }

    websocket.send(JSON.stringify({
        MessageType: "AnomaliesControl",
        State: generate ? "generate" : "stopGenerate"
    }));
}