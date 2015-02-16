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
                path : svg_object,
                legend: svg_object,
                legend_r: svg_object,
		yMin : number,
		yMax : number,
		displayName : string,
		label : string
            }
        },
	svg : svg_object,
        tip: tooltip_object,
        height: '',
        width: '',
        x : d3.scale,
        y0 : d3.scale,
        y1 : d3.scale
    }
};*/

var registeredCharts = {};

// global colors set
var colors = d3.scale.category10()


// worker-related
var sss = null;

// register chart fo futher creation
function registerChart(chartId, dataGUIDs) {
    // initialize object
    registeredCharts[chartId] = {
        dataFlows: {},
        tip: null,
        height: '',
        width: '',
        x: null,
        y0: null,
        y1: null,
        svg: null,
        displayName: ''
    };

    // initialize object
    for (var idx = 0, idxMax = dataGUIDs.length; idx < idxMax; ++idx) {
        registeredCharts[chartId].dataFlows[dataGUIDs[idx]] = {
            data: [],
            path: null,
            legend: null,
            legend_r: null,
        };
    }

    // add color
    colors.domain(dataGUIDs);
}

// Create the chart that will be used to
// display the live data.  We use the D3.js
// library to establish SVG elements that will
// do the rendering

function createChart(chartId) {

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

    var chart = registeredCharts[chartId];
    var dataFlowsArray = [];

    chart.width = container.width() - margin.right;
    chart.height = container.height() - margin.top - margin.bottom;

    // create dataFlows array
    for (var id in chart.dataFlows) {
        dataFlowsArray.push({
            id: id,
            yMin: chart.dataFlows[id].yMin,
            yMax: chart.dataFlows[id].yMax,
            label: chart.dataFlows[id].label,
        });
    }

    // seed the axes with some dummy values

    chart.x = d3.time.scale()
		.domain([new Date("2015-01-01T04:02:39.867841Z"), new Date("2015-01-01T04:07:39.867841Z")])
		.range([0, chart.width]);

    chart.y0 = d3.scale.linear()
		.range([chart.height, 0]);

    if (dataFlowsArray.length > 0 && dataFlowsArray[0].yMax != null && dataFlowsArray[0].yMin != null)
        chart.y0.domain([dataFlowsArray[0].yMin, dataFlowsArray[0].yMax]);

    chart.y1 = d3.scale.linear()
		.range([chart.height, 0]);

    if (dataFlowsArray.length > 1 && dataFlowsArray[1].yMax != null && dataFlowsArray[1].yMin != null)
        chart.y1.domain([dataFlowsArray[1].yMin, dataFlowsArray[1].yMax]);

    chart.svg = d3.select("#" + chartId)
		.append("p")
		.append("svg")
		.attr("width", chart.width + margin.left + margin.right)
		.attr("height", chart.height + margin.top + margin.bottom)
		.style("margin-left", margin.left + "px")
		.style("margin-bottom", margin.bottom + "px")
		.style("margin-right", margin.right + "px")
		.append("g")
		.attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    chart.svg.append("g")
		.attr("class", "y0 axis")
		.call(d3.svg.axis().scale(chart.y0).ticks(7).orient("left"));

    if (dataFlowsArray.length > 0 && dataFlowsArray[0].label) {
        chart.svg.append("text")
            .attr("transform", "rotate(-90)")
            .attr("class", "y0 label")
            .attr("text-anchor", "middle")
            .attr("y", -50)
            .attr("x", -chart.height / 2)
            .attr("dy", "1em")
            .attr("font-size", newFontSize + "px")
            .style("fill", colors(dataFlowsArray[0].id))
            .text(dataFlowsArray[0].label);
    }

    if (dataFlowsArray.length > 1 && dataFlowsArray[1].label) {
        chart.svg.append("text")
			.attr("y1", 0 - (margin.top / 2))

        chart.svg.append("g")
			.attr("class", "y1 axis")
			.attr("transform", "translate(" + chart.width + ",0)")
			.text(dataFlowsArray[1].label)
			.call(d3.svg.axis().scale(chart.y1).ticks(7).orient("right"));

        chart.svg.append("text")
			.attr("transform", "rotate(-90)")
			.attr("class", "y1 label")
			.attr("text-anchor", "middle")
			.attr("y", chart.width + 30)
			.attr("x", -chart.height / 2)
			.attr("dy", "1em")
			.attr("font-size", newFontSize + "px")
            .style("fill", colors(dataFlowsArray[1].id))
			.text(dataFlowsArray[1].label);
    }

    chart.svg.append("g")
		.attr("class", "x axis")
		.attr("transform", "translate(0," + (chart.height) + ")")
		.call(d3.svg.axis().scale(chart.x).orient("bottom").tickFormat(d3.time.format("%X")));

    // create tip
    chart.tip = d3.tip()
        .attr('class', 'd3-tip')
        .offset([-10, 0])
        .html(function (d) {
            return "<label class='time_header'>" + d.time + "</label><label class='value_circle'>&#x25cf;</label><label class='value'>" + d.data.toFixed(2) + "</label><label class='message'>" + d.alertData.message + "</label>";
        });
    chart.svg.call(chart.tip);

    if (!window['resizeCallback@' + chartId]) {
        window['resizeCallback@' + chartId] = true;
        $(window).bind('resize', function () {
            console.log('rezise chart: ' + chartId);
            // remove original one
            RemoveFromChart(chartId);
            d3.select("#" + chartId).select('svg').remove();
            // clean up alert points

            // create a new one w/ correct size
            createChart(chartId);
        });
    }
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

function AddToD3(chartId, chartGUID, val, time, y_axis, alertData) {

    //    if (sensorNames.indexOf(D3_set[i].name) == -1) {
    //        sensorNames.push(D3_set[i].name);
    //    }


    var data = registeredCharts[chartId].dataFlows[chartGUID].data;
    if (data == null || data == undefined) {
        data = registeredCharts[chartId].dataFlows[chartGUID].data = [];
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

function PruneOldD3Data(chartId) {
    var now = new Date();
    var cutoff = new Date(now - WINDOW_MINUTES * MS_PER_MINUTE)

    console.log(now);
    console.log(cutoff);

    // cut data
    for (var id in registeredCharts[chartId].dataFlows) {
        var data = registeredCharts[chartId].dataFlows[id].data;
        var cleared = false;

        while (data.length >= 1 && data[0].time < cutoff) {
            data.shift();
        }
        if (cleared) {
            // clear
            if (registeredCharts[chartId].path[id] != null) {
                registeredCharts[chartId].path[id].remove();
                registeredCharts[chartId].path[id] = null;
            }
            if (registeredCharts[chartId].legend[id] != null) {
                registeredCharts[chartId].legend[id].remove();
                registeredCharts[chartId].legend[id] = null;
            }
            if (registeredCharts[chartId].legend_r[id] != null) {
                registeredCharts[chartId].legend_r[id].remove();
                registeredCharts[chartId].legend_r[id] = null;
            }
        }
    }
}

//
// UpdateD3Chart
//
//  Refreshes D3 charts with new data.  Rebinds data
//  arrays, recomputes ranges for x and y axes
//

function UpdateD3Chart(chartId) {

    var minDate = new Date("3015-01-01T04:02:39.867841Z");
    var maxDate = new Date("1915-01-01T04:02:39.867841Z")

    var minVal = [Number.MAX_VALUE, Number.MAX_VALUE];
    var maxVal = [0, 0];

    var displayHeight = $(window).height();

    var chart = registeredCharts[chartId];

    var dataGUIDs = [];

    for (var id in chart.dataFlows) {
        dataGUIDs.push(id);

        var dataFlow = chart.dataFlows[id];
        var data = dataFlow.data;
        if (data.length == 0 || !dataFlow.displayName) return;

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
    }

    // create chart on demand
    if (chart.svg == null) {
        createChart(chartId);
    }

    if (minVal[0] < Number.MAX_VALUE) {
        var scaleMargin = (maxVal[0] - minVal[0]) * 10 / 100;
        chart.y0 = d3.scale.linear()
			.domain([minVal[0] - scaleMargin, maxVal[0] + scaleMargin])
			.range([chart.height, 0]);

        var yAxisLeft = d3.svg.axis()
			.scale(chart.y0)
			.orient("left")

        chart.svg.selectAll("g.y0.axis")
			.call(yAxisLeft);
    }

    if (minVal[1] < Number.MAX_VALUE) {
        var scaleMargin = (maxVal[1] - minVal[1]) * 10 / 100;

        chart.y1 = d3.scale.linear()
			.domain([minVal[1] - scaleMargin, maxVal[1] + scaleMargin])
			.range([chart.height, 0]);

        var yAxisRight = d3.svg.axis()
			.scale(chart.y1)
			.orient("right")

        chart.svg.selectAll("g.y1.axis")
			.call(yAxisRight);
    }

    chart.x = d3.time.scale()
		.domain([minDate, maxDate])
		.range([0, chart.width]);

    var xAxis = d3.svg.axis()
		.scale(chart.x)
		.tickFormat(d3.time.format("%X"))
		.orient("bottom");

    chart.svg.selectAll("g.x.axis")
		.call(xAxis);

    var line = [
		d3.svg.line()
        .interpolate("monotone")
		.x(function (d) {
		    return chart.x(d.time);
		})
		.y(function (d) {
		    return chart.y0(d.data);
		}),

		d3.svg.line()
        .interpolate("monotone")
		.x(function (d) {
		    return chart.x(d.time);
		})
		.y(function (d) {
		    return chart.y1(d.data);
		})
    ];

    try {
        for (var i = 0, j = dataGUIDs.length; i < j; ++i) {
            var dataGUID = dataGUIDs[i];
            var dataFlow = chart.dataFlows[dataGUID];
            var data = dataFlow.data;

            if (dataFlow.path == null) {
                dataFlow.path = chart.svg.append("g")
                    .append("path")
                    .datum(data)
                    .attr("class", "line")
                    .attr("d", line[data[0].y_axis])
                    .style("stroke", function (d) {
                        return colors(dataGUID);
                    });
            }

            dataFlow.path.datum(data)
                .attr("d", line[data[0].y_axis]);

            // draw alert points
            for (var pnt in data) {
                if (typeof data[pnt].alertData == 'object') {
                    if (data[pnt].alertShowed == undefined) {
                        var transferData = JSON.stringify({ alertData: data[pnt].alertData, time: data[pnt].time, data: data[pnt].data });

                        data[pnt].alertBarShowed = chart.svg.append("g").append("rect")
                            .attr("class", "bar")
                            .attr("x", chart.x(data[pnt].time))
                            .attr("y", 0)
                            .attr("height", chart.height)
                            .attr("width", "2px")
                            .style("fill", "#e6c9cd")

                        data[pnt].alertShowed = chart.svg.append("g").append("circle")
                            .attr("class", "d3-dot")
                            .attr("cx", chart.x(data[pnt].time))
                            .attr("cy", data[pnt].y_axis == 0 ? chart.y0(data[pnt].data) : chart.y1(data[pnt].data))
                            .style("fill", "#e93541")
                            .attr("r", displayHeight / 200)
                            .on('mouseover', function () { d3.select(this).transition().attr("r", displayHeight / 130); eval("chart.tip.show(" + transferData + ");") })
                            .on('mouseout', function () { d3.select(this).transition().attr("r", displayHeight / 200); chart.tip.hide(); });
                    } else {
                        data[pnt].alertShowed.attr("cx", chart.x(data[pnt].time))
                            .attr("cy", data[pnt].y_axis == 0 ? chart.y0(data[pnt].data) : chart.y1(data[pnt].data));

                        data[pnt].alertBarShowed
                            .attr("x", chart.x(data[pnt].time))
                    }
                }
            }
            if (dataFlow.legend == null) {
                dataFlow.legend_r = chart.svg.append("rect")
                    .attr("class", "legend")
                    .attr("width", 10)
                    .attr("height", 10)
                    .attr("x", chart.width + 50)
                    .attr("y", 20 + (20 * i))
                    .style("fill", colors(dataGUID))
                    .style("stroke", colors(dataGUID));

                dataFlow.legend = chart.svg.append("text")
                    .attr("x", chart.width + 65)
                    .attr("y", 20 + (20 * i) + 5)
                    .attr("class", "legend")
                    .style("fill", colors(dataGUID))
                    .text(dataFlow.displayName);
            }
            else {
                dataFlow.legend.text(dataFlow.displayName);
            }
        }
    } catch (e) {
        console.log(e);
    }
}


function RemoveFromChart(chartId) {
    if (!registeredCharts.hasOwnProperty(chartId)) return;

    // clear data
    for (var id in registeredCharts[chartId].dataFlows) {
        dataFlow = registeredCharts[chartId].dataFlows[id];
        if (dataFlow.path) {
            dataFlow.path.remove();
            dataFlow.path = null;
        }
        if (dataFlow.legend) {
            dataFlow.legend.remove();
            dataFlow.legend = null;
        }
        if (dataFlow.legend_r) {
            dataFlow.legend_r.remove();
            dataFlow.legend_r = null;
        }
    }
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
        RemoveFromChart(id);
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
                        $(this).css('color', colors(d == 'All' ? 'avg' : d));
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
                    $(this).css('color', colors(d == 'All' ? 'avg' : d));
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
            $(this).css('color', colors(device));
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

            //var j = $('#sensorList li').eq(0);
            //j.css('color', color('avg'));
            //j.css('font-weight', 'bold');
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
                        if (registeredCharts[id].dataFlows.hasOwnProperty(eventObject.GUID)) {
                            chartId = id;
                            AddToD3(chartId, eventObject.GUID, eventObject.Value, eventObject.timestart, 0, alertData);
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
                            // update graphs
                            PruneOldD3Data(id);
                            UpdateD3Chart(id);
                        }

                        $('#loading').hide();
                        isBulking = false;
                    }
                } else {
                    // the message is data for the charts. find chart for message
                    for (var id in registeredCharts) {
                        if (registeredCharts[id].dataFlows.hasOwnProperty(eventObject.GUID)) {
                            chartId = id;
                            AddToD3(chartId, eventObject.GUID, eventObject.Value, eventObject.time, 0);
                            break;
                        }
                    }
                }
            }
            if (!isBulking && chartId) {
                // remember properties
                SetFlowProperties(chartId, eventObject.GUID, eventObject);

                PruneOldD3Data(chartId);
                UpdateD3Chart(chartId);
            } else {
                $('#loading-sensor').text(eventObject.DisplayName);
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
function SetFlowProperties(chartId, GUID, eventObject) {
    var flow = registeredCharts[chartId].dataFlows[GUID];
    flow.displayName = eventObject.DisplayName;
    flow.label = eventObject.Measure + " (" + eventObject.UnitOfMeasure + ")";
}