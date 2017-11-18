//http://bl.ocks.org/andredumas/edf630690c10b89be390

tradliteApp.component("techanChart", {
    templateUrl: "/app/components/techanChart/techanChart.html",
    bindings: {
        candles: '<',
        buyIndicies: '<',
        sellIndicies: '<',
        tickerLabel: '<'
    },
    controller: function ($scope, $window) {
        var width;
        var dim;
        var parseDate;
        var zoom;
        var x;
        var y;
        var yPercent;
        var yInit, zoomableInit; //yPercentInit,
        var yVolume;
        var candlestick;
        var tradearrow;
        var volume;
        var trendline;
        var supstance;
        var xAxis;
        var timeAnnotation;
        var yAxis;
        var ohlcAnnotation;
        var closeAnnotation;
        var percentAxis;
        var percentAnnotation;
        var volumeAxis;
        var volumeAnnotation;
        var ohlcCrosshair;
        var svg;
        var defs;
        var ohlcSelection;
        var indicatorSelection;
        var candles;
        var buyRules;
        var sellRules;
        var self = this;
        this.$onChanges = function () {
            if (self.candles && self.candles.length > 0) {
                candles = [];
                buyRules = [];
                sellRules = [];
                d3.select("#chart").selectAll("*").remove();
                initChart();
                candles = self.candles;
                if (self.buyIndicies) {
                    buyRules = self.buyIndicies;
                }
                if (self.sellIndicies) {
                    sellRules = self.sellIndicies;
                }
                drawChart();
            }
        }

        function zoomed() {
            x.zoomable().domain(d3.event.transform.rescaleX(zoomableInit).domain());
            y.domain(d3.event.transform.rescaleY(yInit).domain());
            //yPercent.domain(d3.event.transform.rescaleY(yPercentInit).domain());

            draw();
        }

        function draw() {
            svg.select("g.x.axis").call(xAxis);
            svg.select("g.ohlc .axis").call(yAxis);
            svg.select("g.volume.axis").call(volumeAxis);
            svg.select("g.percent.axis").call(percentAxis);

            // We know the data does not change, a simple refresh that does not perform data joins will suffice.
            svg.select("g.candlestick").call(candlestick.refresh);
            svg.select("g.close.annotation").call(closeAnnotation.refresh);
            svg.select("g.volume").call(volume.refresh);
            svg.select("g.tradearrow").call(tradearrow.refresh);
            svg.select("g.supstances").call(supstance.refresh);
        }

        function drawChart() {
            console.log(candles);
            console.log(buyRules);
            console.log(sellRules);
            var accessor = candlestick.accessor(),
                indicatorPreRoll = 33;  // Don't show where indicators don't have data
            var data = candles.map(function (candle) {
                return {
                    date: parseDate(candle.dateTime.substring(0, candle.dateTime.indexOf('+'))),
                    open: candle.open,
                    high: candle.high,
                    low: candle.low,
                    close: candle.close,
                    volume: candle.volume
                };
            }).sort(function (a, b) { return d3.ascending(accessor.d(a), accessor.d(b)); });

            x.domain(techan.scale.plot.time(data).domain());
            y.domain(techan.scale.plot.ohlc(data).domain()); //.slice(indicatorPreRoll)
            //yPercent.domain(techan.scale.plot.percent(y, accessor(data[indicatorPreRoll])).domain());
            yVolume.domain(techan.scale.plot.volume(data).domain());

            //var trendlineData = [
            //    { start: { date: new Date(2014, 2, 11), value: 72.50 }, end: { date: new Date(2014, 5, 9), value: 63.34 } },
            //    { start: { date: new Date(2013, 10, 21), value: 43 }, end: { date: new Date(2014, 2, 17), value: 70.50 } }
            //];

            //var startDate = data[0].date;
            //var endDate = data[data.length-1].date

            //var supstanceData = [
            //    { start: startDate, end: endDate, value: 63.64 },
            //    { start: startDate, end: endDate, value: 55.50 }
            //];

            var buySignals = buyRules.map(function (index) {
                return {
                    date: data[index].date,
                    type: "buy",
                    price: data[index].low,
                    low: data[index].low,
                    high: data[index].high
                }
            });
            var sellSignal = sellRules.map(function (index) {
                return {
                    date: data[index].date,
                    type: "sell",
                    price: data[index].high,
                    low: data[index].low,
                    high: data[index].high
                }
            });
            var signals = sellSignal.concat(buySignals);
            svg.select("g.candlestick").datum(data).call(candlestick);
            svg.select("g.volume").datum(data).call(volume);

            svg.select("g.crosshair.ohlc").call(ohlcCrosshair).call(zoom);

            svg.select("g.tradearrow").datum(signals).call(tradearrow);
            svg.append('text')
                .attr("class", "symbol")
                .attr("x", 20)
                .text(self.tickerLabel);

            //svg.select("g.supstances").datum(supstanceData).call(supstance).call(supstance.drag);
            // Stash for zooming
            zoomableInit = x.zoomable().domain([0, data.length]).copy(); // Zoom in a little to hide indicator preroll
            yInit = y.copy();
            //yPercentInit = yPercent.copy();

            draw();
        }
        
        function initChart() {
            width = document.getElementById("chart").offsetWidth;
            height = $window.innerHeight - 150;
            dim = {
                width: width, height: height,
                margin: { top: 20, right: 50, bottom: 30, left: 50 },
                ohlc: { height: height - 60 }
            };
            dim.plot = {
                width: dim.width - dim.margin.left - dim.margin.right,
                height: dim.height - dim.margin.top - dim.margin.bottom
            };

            parseDate = d3.timeParse("%Y-%m-%dT%H:%M:%S");

            zoom = d3.zoom()
                .on("zoom", zoomed);

            x = techan.scale.financetime()
                .range([0, dim.plot.width]);

            y = d3.scaleLinear()
                .range([dim.ohlc.height, 0]);


            yPercent = y.copy();   // Same as y at this stage, will get a different domain later

            

            yVolume = d3.scaleLinear()
                .range([y(0), y(0.4)]);

            candlestick = techan.plot.candlestick()
                .xScale(x)
                .yScale(y);

            tradearrow = techan.plot.tradearrow()
                .xScale(x)
                .yScale(y)
                .y(function (d) {
                    // Display the buy and sell arrows a bit above and below the price, so the price is still visible
                    if (d.type === 'buy') return y(d.low) + 5;
                    if (d.type === 'sell') return y(d.high) - 5;
                    else return y(d.price);
                });

            volume = techan.plot.volume()
                .accessor(candlestick.accessor())   // Set the accessor to a ohlc accessor so we get highlighted bars
                .xScale(x)
                .yScale(yVolume);

            trendline = techan.plot.trendline()
                .xScale(x)
                .yScale(y);

            supstance = techan.plot.supstance()
                .xScale(x)
                .yScale(y);

            xAxis = d3.axisBottom(x);

            timeAnnotation = techan.plot.axisannotation()
                .axis(xAxis)
                .orient('bottom')
                .format(d3.timeFormat('%Y-%m-%d'))
                .width(65)
                .translate([0, dim.plot.height]);

            yAxis = d3.axisRight(y);

            ohlcAnnotation = techan.plot.axisannotation()
                .axis(yAxis)
                .orient('right')
                .format(d3.format(',.2f'))
                .translate([x(1), 0]);

            closeAnnotation = techan.plot.axisannotation()
                .axis(yAxis)
                .orient('right')
                .accessor(candlestick.accessor())
                .format(d3.format(',.2f'))
                .translate([x(1), 0]);

            percentAxis = d3.axisLeft(yPercent)
                .tickFormat(d3.format('+.1%'));

            percentAnnotation = techan.plot.axisannotation()
                .axis(percentAxis)
                .orient('left');

            volumeAxis = d3.axisRight(yVolume)
                .ticks(3)
                .tickFormat(d3.format(",.3s"));

            volumeAnnotation = techan.plot.axisannotation()
                .axis(volumeAxis)
                .orient("right")
                .width(35);

            ohlcCrosshair = techan.plot.crosshair()
                .xScale(timeAnnotation.axis().scale())
                .yScale(ohlcAnnotation.axis().scale())
                .xAnnotation(timeAnnotation)
                .yAnnotation([ohlcAnnotation, percentAnnotation, volumeAnnotation])
                .verticalWireRange([0, dim.plot.height]);

            svg = d3.select("#chart").append("svg")
                .attr("width", dim.width)
                .attr("height", dim.height);

            defs = svg.append("defs");

            defs.append("clipPath")
                .attr("id", "ohlcClip")
                .append("rect")
                .attr("x", 0)
                .attr("y", 0)
                .attr("width", dim.plot.width)
                .attr("height", dim.ohlc.height);

            svg = svg.append("g")
                .attr("transform", "translate(" + dim.margin.left + "," + dim.margin.top + ")");

            svg.append("g")
                .attr("class", "x axis")
                .attr("transform", "translate(0," + dim.plot.height + ")");

            ohlcSelection = svg.append("g")
                .attr("class", "ohlc")
                .attr("transform", "translate(0,0)");

            ohlcSelection.append("g")
                .attr("class", "axis")
                .attr("transform", "translate(" + x(1) + ",0)")
                .append("text")
                .attr("transform", "rotate(-90)")
                .attr("y", -12)
                .attr("dy", ".71em")
                .style("text-anchor", "end")
                .text("Price");

            ohlcSelection.append("g")
                .attr("class", "close annotation up");

            ohlcSelection.append("g")
                .attr("class", "volume")
                .attr("clip-path", "url(#ohlcClip)");

            ohlcSelection.append("g")
                .attr("class", "candlestick")
                .attr("clip-path", "url(#ohlcClip)");

            ohlcSelection.append("g")
                .attr("class", "indicator sma ma-0")
                .attr("clip-path", "url(#ohlcClip)");

            ohlcSelection.append("g")
                .attr("class", "indicator sma ma-1")
                .attr("clip-path", "url(#ohlcClip)");

            ohlcSelection.append("g")
                .attr("class", "indicator ema ma-2")
                .attr("clip-path", "url(#ohlcClip)");

            ohlcSelection.append("g")
                .attr("class", "percent axis");

            ohlcSelection.append("g")
                .attr("class", "volume axis");

            indicatorSelection = svg.selectAll("svg > g.indicator").data(["macd", "rsi"]).enter()
                .append("g")
                .attr("class", function (d) { return d + " indicator"; });

            indicatorSelection.append("g")
                .attr("class", "axis right")
                .attr("transform", "translate(" + x(1) + ",0)");

            indicatorSelection.append("g")
                .attr("class", "axis left")
                .attr("transform", "translate(" + x(0) + ",0)");

            indicatorSelection.append("g")
                .attr("class", "indicator-plot")
                .attr("clip-path", function (d, i) { return "url(#indicatorClip-" + i + ")"; });

            // Add trendlines and other interactions last to be above zoom pane
            svg.append('g')
                .attr("class", "crosshair ohlc");

            svg.append("g")
                .attr("class", "tradearrow")
                .attr("clip-path", "url(#ohlcClip)");

            svg.append('g')
                .attr("class", "crosshair macd");

            svg.append('g')
                .attr("class", "crosshair rsi");

            svg.append("g")
                .attr("class", "trendlines analysis")
                .attr("clip-path", "url(#ohlcClip)");
            svg.append("g")
                .attr("class", "supstances analysis")
                .attr("clip-path", "url(#ohlcClip)");

            if (candles) {
                drawChart();
            }
        };
        
        this.$onInit = function(){
            angular.element($window).bind('resize', function () {
                d3.select("#chart").selectAll("*").remove();
                initChart();
            });
        }        
    }
});