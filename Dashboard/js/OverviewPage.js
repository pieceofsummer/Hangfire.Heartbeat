"use strict";

// MODEL

var UtilizationViewModel = function () {
    var self = this;
    self.servers = ko.observableArray();
};
var viewModel = new UtilizationViewModel();

// UPDATER
var updater = function (cpuGraph, memGraph) {
    var config = $("#heartbeatConfig"),
        timeout = config.data("pollinterval");

    (function cb() {
        $.get(config.data("pollurl"), function (data) {
            var cpuGraphData = {};
            var memGraphData = {};
            var newServerViews = [];

            for (var i = 0; i < data.length; i++) {

                var current = data[i];
                var name = current.name;

                var server = getServerView(name, current);
                server.displayColor(getColor(name, cpuGraph.series));

                newServerViews.push(server);

                cpuGraphData[name] = current.cpuUsagePercentage;
                memGraphData[name] = current.workingMemorySet;
                
                // add data series items if needed
                addServerItem(cpuGraph.series, name, current.displayName);
                addServerItem(memGraph.series, name, current.displayName);
            }

            cpuGraph.series.addData(cpuGraphData);
            cpuGraph.update();

            memGraph.series.addData(memGraphData);
            memGraph.update();

            viewModel.servers(newServerViews);

            // set configured timeout on success
            timeout = config.data("pollinterval");
        }).fail(function () {
            // incremental back-off on failure
            timeout = Math.min(timeout * 1.5, 30000);
        }).always(function () {
            setTimeout(cb, timeout);
        });
    })();
};

var createGraph = function (elementName, yAxisConfig) {
    var graph = new Rickshaw.Graph({
        element: document.getElementById(elementName),
        height: 300,
        renderer: 'area',
        interpolation: 'cardinal',
        unstack: true,
        stroke: true,
        padding: { top: 0.2, left: 0, right: 0, bottom: 0.2 },
        series: new Rickshaw.Series.FixedDuration([{ name: "__STUB" }],
            { scheme: 'cool' },
            {
                timeInterval: 1000,
                maxDataPoints: 60
            })
    });

    var xAxis = new Rickshaw.Graph.Axis.X({
        graph: graph,
        ticksTreatment: 'glow',
        tickFormat: function (x) { return ''; },
        ticks: 10,
        timeUnit: 'second'
    });
    xAxis.render();

    var yAxis = yAxisConfig(graph);
    yAxis.render();

    $.data(graph.element, 'graph', graph);
    return graph;
};

var getServerView = function (name, current) {

    var server = ko.utils.arrayFirst(viewModel.servers(),
        function (s) { return s.name === name; });

    var cpuUsage = numeral(current.cpuUsagePercentage).format('0.0') + '%';
    var ramUsage = numeral(current.workingMemorySet).format('0.00ib');

    if (server == null) {
        server = {
            displayName: current.displayName,
            displayColor: ko.observable("#000000"),
            name: name,
            processId: ko.observable(current.processId),
            processName: ko.observable(current.processName),
            cpuUsage: ko.observable(cpuUsage),
            ramUsage: ko.observable(ramUsage)
        };
    } else {
        server.processId(current.processId);
        server.processName(current.processName);
        server.cpuUsage(cpuUsage);
        server.ramUsage(ramUsage);
    }

    return server;
};

var getColor = function (name, graphSeries) {
    var series = ko.utils.arrayFirst(graphSeries,
        function (s) { return s.name === name; });

    return series != null ? series.color : "transparent";
};

var formatDate = function (unixSeconds) {
    return moment(unixSeconds * 1000).format("H:mm:ss");
};

var addServerItem = function (series, name, title) {
    var item = series.itemByName(name);
    if (item) return;
    
    series.addItem({ name: name, title: title });
};

// INITIALIZATION
window.onload = function () {

    var cpuGraph = createGraph("cpu-chart",
        function (graph) {
            return new Rickshaw.Graph.Axis.Y({
                graph: graph,
                tickFormat: function (y) { return y !== 0 ? numeral(y).format('0.0') + '%' : ''; },
                ticksTreatment: 'glow'
            });
        });

    var memGraph = createGraph("mem-chart",
        function (graph) {
            return new Rickshaw.Graph.Axis.Y({
                graph: graph,
                tickFormat: function (y) { return y !== 0 ? numeral(y).format('0.0ib') : ''; },
                ticksTreatment: 'glow'
            });
        });

    var cpuHoverDetail = new Rickshaw.Graph.HoverDetail({
        graph: cpuGraph,
        formatter: function (series, x, y) {
            var date = '<span class="date">' + formatDate(x) + '</span>';
            if (series.name === "__STUB") return date;

            var swatch = '<span class="detail_swatch" style="background-color: ' + series.color + '"></span>';
            var content = swatch + series.title + ': <span class="value">' + numeral(y).format('0.0') + '%</span>';
            return content;
        }
    });

    var memHoverDetail = new Rickshaw.Graph.HoverDetail({
        graph: memGraph,
        formatter: function (series, x, y) {
            var date = '<span class="date">' + formatDate(x) + '</span>';
            if (series.name === "__STUB") return date;

            var swatch = '<span class="detail_swatch" style="background-color: ' + series.color + '"></span>';
            var content = swatch + series.title + ': <span class="value">' + numeral(y).format('0.00ib') + '</span>';
            return content;
        }
    });

    updater(cpuGraph, memGraph);
    ko.applyBindings(viewModel);
    
    $(window).on("resize", function () {
        $(".rickshaw_graph").each(function () {
            var container = $(this),
                graph = container.data('graph');
            
            if (graph) {
                var width = container.width(),
                    height = container.height();
                
                if (graph.width !== width || graph.height !== height) {
                    // container size has changed, update graph size
                    graph.setSize({ width: width, height: height });
                    graph.update();
                }
            }
        });
    });
};
