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

/*
registeredCharts = {
    chartId : {
        dataFlows : {
            flowUUID : {
		data : [],
        	    svg: svg_object,
                path : svg_object,
                legend: svg_object,
                legend_r: svg_object,
                color : svg_object,
            }
        },
        tip: tooltip_object,
        height: '',
        width: '',
        x : d3.scale,
        y0 : d3.scale,
        y1 : d3.scale
    }
};
*/

var registeredCharts = {};

// worker-related
var sss = null;

// register chart fo futher creation
function registerChart(chartId, dataUUIDs) {
    // just copy
    registeredCharts[chartId].dataUUIDs = dataUUIDs;

    // initialize object
    for (var id in dataUUIDs) {
        registeredCharts[chartId].dataFlows[id] = {
            data: [],
            svg: null,
            path: null,
            legend: null,
            legend_r: null,
            color: null

        };

        var flow = registeredCharts[chartId].dataFlows[id];
        // initialize color
        flow.color = d3.scale.category10();
        flow.color.domain(id);
    }
}

// Create the chart(s) that will be used to
// display the live data.  We use the D3.js
// library to establish SVG elements that will
// do the rendering

function chart(chartId, y0_label, y0_min, y0_max, y1_label, y1_min, y1_max) {

    var margin = {
        top: 30,
        right: 250,
        bottom: 20,
        left: 50
    };

    var container = $('#' + chartId);

    //Standard height, for which the body font size is correct
    var preferredHeight = 768;
    //Base font size for the page
    var fontsize = 12;

    var displayHeight = $(window).height();
    var percentage = displayHeight / preferredHeight;
    var newFontSize = Math.floor(fontsize * percentage) - 1;

    registeredCharts[chartId].width = container.width() - margin.right;
    registeredCharts[chartId].height = container.height() - margin.top - margin.bottom;

    // seed the axes with some dummy values

    registeredCharts[chartId].x = d3.time.scale()
		.domain([new Date("2015-01-01T04:02:39.867841Z"), new Date("2015-01-01T04:07:39.867841Z")])
		.range([0, registeredCharts[chartId].width]);

    registeredCharts[chartId].y0 = d3.scale.linear()
		.domain([y0_min, y0_max])
		.range([registeredCharts[chartId].height, 0]);

    registeredCharts[chartId].y1 = d3.scale.linear()
		.domain([y1_min, y1_max])
		.range([registeredCharts[chartId].height, 0]);

    registeredCharts[chartId].svg = d3.select("#" + chartId)
		.append("p")
		.append("svg")
		.attr("width", registeredCharts[chartId].width + margin.left + margin.right)
		.attr("height", registeredCharts[chartId].height + margin.top + margin.bottom)
		.style("margin-left", margin.left + "px")
		.style("margin-bottom", margin.bottom + "px")
		.style("margin-right", margin.right + "px")
		.append("g")
		.attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    registeredCharts[chartId].svg.append("g")
		.attr("class", "y0 axis")
		.call(d3.svg.axis().scale(registeredCharts[chartId].y0).ticks(7).orient("left"));

    if (y0_label) {
        registeredCharts[chartId].svg.append("text")
            .attr("transform", "rotate(-90)")
            .attr("class", "y0 label")
            .attr("text-anchor", "middle")
            .attr("y", -50)
            .attr("x", -registeredCharts[chartId].height / 2)
            .attr("dy", "1em")
            .attr("font-size", newFontSize + "px")
            .text(y0_label);
    }

    if (y1_label) {
        registeredCharts[chartId].svg.append("text")
			.attr("y1", 0 - (margin.top / 2))

        registeredCharts[chartId].svg.append("g")
			.attr("class", "y1 axis")
			.attr("transform", "translate(" + registeredCharts[chartId].width + ",0)")
			.text(y1_label)
			.call(d3.svg.axis().scale(registeredCharts[chartId].y1).ticks(7).orient("right"));

        registeredCharts[chartId].svg.append("text")
			.attr("transform", "rotate(-90)")
			.attr("class", "y1 label")
			.attr("text-anchor", "middle")
			.attr("y", registeredCharts[chartId].width + 30)
			.attr("x", -registeredCharts[chartId].height / 2)
			.attr("dy", "1em")
			.attr("font-size", newFontSize + "px")
			.text(y1_label);
    }

    registeredCharts[chartId].svg.append("g")
		.attr("class", "x axis")
		.attr("transform", "translate(0," + (registeredCharts[chartId].height) + ")")
		.call(d3.svg.axis().scale(registeredCharts[chartId].x).orient("bottom").tickFormat(d3.time.format("%X")));

    // create tip
    tip[chartId] = d3.tip()
        .attr('class', 'd3-tip')
        .offset([-10, 0])
        .html(function (d) {
            return "<label class='time_header'>" + d.time + "</label><label class='value_circle'>&#x25cf;</label><label class='value'>" + d.data.toFixed(2) + "</label><label class='message'>" + d.alertData.message + "</label>";
        });
    registeredCharts[chartId].svg.call(tip[chartId]);

    if (!window['resizeCallback@' + chartId]) {
        window['resizeCallback@' + chartId] = true;
        $(window).bind('resize', function () {
            console.log('rezise chart: ' + chartId);
            // remove original one
            d3.select("#" + chartId).select('svg').remove();
            // clean up alert points

            // create a new one w/ correct size
            chart(chartId, y0_label, y0_min, y0_max, y1_label, y1_min, y1_max);
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

function MakeNewD3Series(chartId, chartUUID) {

    var obj = {
        name: chartUUID,
        data: []
    };
    registeredCharts[chartId].dataFlows[chartUUID].push(obj);

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

function AddToD3(chartId, chartUUID, val, time, y_axis, alertData) {

    //    if (sensorNames.indexOf(D3_set[i].name) == -1) {
    //        sensorNames.push(D3_set[i].name);
    //    }


    var data = registeredCharts[chartId].dataFlows.data;
    if (data == null || data == undefined) {
        data = MakeNewD3Series(chartId, chartUUID).data;
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

function PruneOldD3Data(chartId, dataUUID) {
    var now = new Date();
    var cutoff = new Date(now - WINDOW_MINUTES * MS_PER_MINUTE)

    console.log(now);
    console.log(cutoff);

    // cut data
    var charArray = registeredCharts[chartId].set[dataUUID];

    while (charArray.length >= 1 && charArray[0].time < cutoff) {
        charArray.shift();
        // clear
        if (registeredCharts[chartId].path[dataUUID] != null) {
            registeredCharts[chartId].path[dataUUID].remove();
            registeredCharts[chartId].path[dataUUID] = null;
        }
        if (registeredCharts[chartId].legend[dataUUID] != null) {
            registeredCharts[chartId].legend[dataUUID].remove();
            registeredCharts[chartId].legend[dataUUID] = null;
        }
        if (registeredCharts[chartId].legend_r[dataUUID] != null) {
            registeredCharts[chartId].legend_r[dataUUID].remove();
            registeredCharts[chartId].legend_r[dataUUID] = null;
        }
    }
}

//
// UpdateD3Charts
//
//  Refreshes D3 charts with new data.  Rebinds data
//  arrays, recomputes ranges for x and y axes
//

function UpdateD3Charts(chartId, dataUUID) {

    var minDate = new Date("3015-01-01T04:02:39.867841Z");
    var maxDate = new Date("1915-01-01T04:02:39.867841Z")

    var minVal = [Number.MAX_VALUE, Number.MAX_VALUE];
    var maxVal = [0, 0];

    var displayHeight = $(window).height();

    var data = registeredCharts[chartId].dataFlows[dataUUID].data;
    // sort data
    data.sort(function (a, b) {
        if (a.time < b.time) return -1;
        if (a.time > b.time) return 1;
        return 0;
    });

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

    color.domain(sensorNames);

    if (minVal[0] < Number.MAX_VALUE) {
        var scaleMargin = (maxVal[0] - minVal[0]) * 10 / 100;
        registeredCharts[chartId].y0 = d3.scale.linear()
			.domain([minVal[0] - scaleMargin, maxVal[0] + scaleMargin])
			.range([registeredCharts[chartId].height, 0]);

        var yAxisLeft = d3.svg.axis()
			.scale(registeredCharts[chartId].y0)
			.orient("left")

        svg[chart_name].selectAll("g.y0.axis")
			.call(yAxisLeft);
    }

    if (minVal[1] < Number.MAX_VALUE) {
        var scaleMargin = (maxVal[1] - minVal[1]) * 10 / 100;

        registeredCharts[chartId].y1 = d3.scale.linear()
			.domain([minVal[1] - scaleMargin, maxVal[1] + scaleMargin])
			.range([registeredCharts[chartId].height, 0]);

        var yAxisRight = d3.svg.axis()
			.scale(registeredCharts[chartId].y1)
			.orient("right")

        svg[chart_name].selectAll("g.y1.axis")
			.call(yAxisRight);
    }

    registeredCharts[chartId].x = d3.time.scale()
		.domain([minDate, maxDate])
		.range([0, registeredCharts[chartId].width]);

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
		    return registeredCharts[chartId].y0(d.data);
		}),

		d3.svg.line()
        .interpolate("monotone")
		.x(function (d) {
		    return x(d.time);
		})
		.y(function (d) {
		    return registeredCharts[chartId].y1(d.data);
		})
    ];

    var dataFlow = registeredCharts[chartId].dataFlows[dataUUID];

    try {

        if (dataFlow.path == null) {
            dataFlow.path = dataFlow.svg.append("g")
				.append("path")
				.datum(data)
				.attr("class", "line")
				.attr("d", line[data[0].y_axis])
				.style("stroke", function (d) {
				    return color(dataUUID);
				});
        }

        dataFlow.path.datum(data)
            .attr("d", line[data[0].y_axis]);

        // draw alert points
        for (var pnt in data) {
            if (typeof data[pnt].alertData == 'object') {
                if (data[pnt].alertShowed == undefined) {
                    var transferData = JSON.stringify({ alertData: data[pnt].alertData, time: data[pnt].time, data: data[pnt].data });

                    data[pnt].alertBarShowed = dataFlow.svg.append("g").append("rect")
                        .attr("class", "bar")
						.attr("x", x(data[pnt].time))
						.attr("y", 0)
                        .attr("height", registeredCharts[chartId].height)
						.attr("width", "2px")
                        .style("fill", "#e6c9cd")

                    data[pnt].alertShowed = dataFlow.svg.append("g").append("circle")
                        .attr("class", "d3-dot")
						.attr("cx", x(data[pnt].time))
						.attr("cy", data[pnt].y_axis == 0 ? registeredCharts[chartId].y0(data[pnt].data) : registeredCharts[chartId].y1(data[pnt].data))
						.style("fill", "#e93541")
						.attr("r", displayHeight / 200)
                        .on('mouseover', function () { d3.select(this).transition().attr("r", displayHeight / 130); eval("tip[chart_name].show(" + transferData + ");") })
                        .on('mouseout', function () { d3.select(this).transition().attr("r", displayHeight / 200); tip[chart_name].hide(); });
                } else {
                    data[pnt].alertShowed.attr("cx", x(data[pnt].time))
						.attr("cy", data[pnt].y_axis == 0 ? registeredCharts[chartId].y0(data[pnt].data) : registeredCharts[chartId].y1(data[pnt].data));

                    data[pnt].alertBarShowed
                        .attr("x", x(data[pnt].time))
                }
            }
        }

        if (dataFlow.legend == null) {
            dataFlow.legend_r = dataFlow.svg.append("rect")
				.attr("class", "legend")
				.attr("width", 10)
				.attr("height", 10)
				.attr("x", registeredCharts[chartId].width + 50)
				.attr("y", 20 + (20 * i))
				.style("fill", color(name))
				.style("stroke", color(name));

            var legend_display_name = name;
            if (name.toLowerCase().indexOf("return_temp") != -1) legend_display_name = "Return Temp";
            else if (name.toLowerCase().indexOf("supply_temp") != -1) legend_display_name = "Supply Temp";
            else if (name.toLowerCase().indexOf("pressure") != -1) legend_display_name = "Pressure";
            else if (name.toLowerCase().indexOf("flow") != -1) legend_display_name = "Flow";

            dataFlow.legend = dataFlow.svg.append("text")
				.attr("x", registeredCharts[chartId].width + 65)
				.attr("y", 20 + (20 * i) + 5)
				.attr("class", "legend")
				.style("fill", color(name))
				.text(legend_display_name == 'avg' ? 'avg (of all sensors)' : legend_display_name);

        }
    } catch (e) {
        console.log(e);
    }
}


function RemoveFromChart(chartId, series_name) {

    path[chartId][series_name].remove();
    path[chartId][series_name] = null;

    legend[chartId][series_name].remove();
    legend[chartId][series_name] = null;

    legend_r[chartId][series_name].remove();
    legend_r[chartId][series_name] = null;
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
    for (var id in registeredCharts) {
        RemoveFromChart(id, registeredCharts[id].name);
        ResetChart(id);
        registeredCharts[id] = { dataUUIDs: registeredCharts[id].dataUUIDs }
    }
}

//
// JQuery ready function
//

$(document).ready(function () {
    // 
    //  Handle a sensor selection change
    // 'All' means all dataset are shown.
    //  Anything else toggles that particular
    //  dataset
    //

    $('#sensorList').on('click', 'li', function () {
        var device = $(this).text();
        if (websocket != null) {

            $('#loading').show();
            ClearD3Charts();

            if (device == 'All') {

                var c = { MessageType: "LiveDataSelection", DeviceName: "clear" };
                websocket.send(JSON.stringify(x));

                var x = { MessageType: "LiveDataSelection", DeviceName: device };
                websocket.send(JSON.stringify(x));

                $('#sensorList li').each(function () {
                    //this now refers to each li
                    //do stuff to each

                    var d = $(this).text();

                    if (d == device) {
                        $(this).addClass('selected');
                        $(this).css('color', color(d == 'All' ? 'avg' : d));
                        $(this).css('font-weight', 'bold');
                    }
                    else {
                        $(this).removeClass('selected');
                        $(this).css('color', '');
                        $(this).css('font-weight', 'normal');
                    }
                });

            }
            else {

                // not 'All' so general case

                var j = $('#sensorList li').eq(0);
                j.removeClass('selected');
                j.css('color', '');
                j.css('font-weight', 'normal');

                var d = $(this).text();
                if ($(this).hasClass('selected')) {
                    $(this).removeClass('selected');
                    $(this).css('color', '');
                    $(this).css('font-weight', 'normal');
                }
                else {
                    $(this).addClass('selected');
                    $(this).css('color', color(d == 'All' ? 'avg' : d));
                    $(this).css('font-weight', 'bold');
                }

                var x = { MessageType: "LiveDataSelection", DeviceName: "clear" };
                websocket.send(JSON.stringify(x));

                $('#sensorList li').each(function () {

                    var d = $(this).text();

                    if ($(this).hasClass('selected')) {
                        var x = { MessageType: "LiveDataSelection", DeviceName: $(this).text() };
                        websocket.send(JSON.stringify(x));
                    }
                });
            }

        }
    });

    // 
    //  Mouseover: highlight the sensor with its color
    //  and make the text bold.
    //

    $('#sensorList').on('mouseover', 'li', function (e) {
        var device = $(this).text();

        if (device == 'All') {
            device = 'avg';
        }

        if ($(this).hasClass('selected') == false) {
            $(this).css('color', color(device));
            $(this).css('font-weight', 'bold');
        }

    }).on('mouseout', 'li', function (e) {
        if ($(this).hasClass('selected') == false) {
            $(this).css('color', '');
            $(this).css('font-weight', 'normal');
        }
    });

    $('#loading').hide();

    //
    // Set up jQuery DataTable to show alerts
    //

    var table = $('#alertTable').DataTable({
        "bAutoWidth": false,
        "bFilter": true,
        "bInfo": true,
        "paging": true,
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

        // initialize the page with all sensors

        if (receivedFirstMessage == false) {

            ClearD3Charts();
            var x = {
                MessageType: "LiveDataSelection",
                DeviceName: 'All'
            };

            websocket.send(JSON.stringify(x));
            receivedFirstMessage = true;

            // make 'All' the active sensor

            var j = $('#sensorList li').eq(0);
            j.css('color', color('avg'));
            j.css('font-weight', 'bold');
        }

        // Seems like we have valid data
        try {

            if (eventObject.DisplayName != null) {

                // if we have a new sensor, add it to the list
                var exists = true;
                if ($('#sensorList').data(eventObject.DisplayName) == undefined) {
                    exists = false;
                }

                if (exists == false) {

                    var ul = document.getElementById("sensorList");
                    var li = document.createElement("li");
                    li.appendChild(document.createTextNode(eventObject.DisplayName));
                    ul.appendChild(li);

                    $('#sensorList').data(eventObject.DisplayName, eventObject.DisplayName);

                }
            }
            var chartId = undefined;

            // If the message is an alert, we need to display it in the datatable
            if (eventObject.alerttype != null) { // && isBulking == false) {
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
                    for (var id in registeredCharts) {
                        if (registeredCharts[id].hasOwnProperty(eventObject.UUID)) {
                            chartId = id;
                            AddToD3(chartId, eventObject.UUID, eventObject.Value, eventObject.timestart, 0, alertData);
                            break;
                        }
                    }
                }
            } else {

                // Message received is not an alert. let's display it in the charts

                if (eventObject.bulkData != null) {

                    // Don't update while receiving bulk data.  It will
                    // cause the browser to (usually) freeze

                    if (eventObject.bulkData == true) {
                        $('#loading').show();
                        isBulking = true;
                        ClearD3Charts();
                    } else {
                        // update all
                        for (var id in registeredCharts) {
                            for (var uuid in registeredCharts[id]) {
                                PruneOldD3Data(id, uuid);
                                UpdateD3Charts(id, uuid);
                            }
                        }

                        $('#loading').hide();
                        isBulking = false;
                    }
                } else {
                    // the message is data for the charts. find chart for message
                    for (var id in registeredCharts) {
                        if (registeredCharts[id].hasOwnProperty(eventObject.UUID)) {
                            chartId = id;
                            AddToD3(chartId, eventObject.UUID, eventObject.Value, eventObject.timestart, 0);
                            break;
                        }
                    }
                }
            }
            if (!isBulking && chartId) {
                PruneOldD3Data(chartId, eventObject.UUID);
                UpdateD3Charts(chartId, eventObject.UUID);
            } else {
                $('#loading-sensor').text(sensorName);
            }

        } catch (e) {

            $('#messages').prepend('<div>Error processing message: ' + e.message + "</div>");
        }
    }

});

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