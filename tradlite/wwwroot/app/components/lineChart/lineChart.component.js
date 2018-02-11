//https://bl.ocks.org/EfratVil/92f894ac0ba265192411e73f633a3e2f

tradliteApp.component("lineChart", {
    templateUrl: "/app/components/lineChart/lineChart.html",
    bindings: {
        lineChartData: '<'
    },
    controller: function ($scope, $window) {
        var self = this;
        this.$onChanges = function () {
            if (self.lineChartData && self.lineChartData.length > 0) {
                console.log(self.lineChartData);
                drawChart();
            }
        }
        function drawChart() {
            var svg = d3.select("svg"),
                margin = { top: 20, right: 20, bottom: 30, left: 50 },
                width = +svg.attr("width") - margin.left - margin.right,
                height = +svg.attr("height") - margin.top - margin.bottom,
                g = svg.append("g").attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            var parseTime = d3.timeParse("%d-%b-%y");

            var x = d3.scaleTime()
                .rangeRound([0, width]);

            var y = d3.scaleLinear()
                .rangeRound([height, 0]);

            var line = d3.line()
                .x(function (d) { return x(d.date); })
                .y(function (d) { return y(d.yValue); });


            x.domain(d3.extent(self.lineChartData, function (d) { return d.date; }));
            y.domain(d3.extent(self.lineChartData, function (d) { return d.yValue; }));

            g.append("g")
                .attr("transform", "translate(0," + height + ")")
                .call(d3.axisBottom(x))
                .select(".domain")
                .remove();

            g.append("g")
                .call(d3.axisLeft(y))
                .append("text")
                .attr("fill", "#000")
                .attr("transform", "rotate(-90)")
                .attr("y", 6)
                .attr("dy", "0.71em")
                .attr("text-anchor", "end")
                .text("Price ($)");

            g.append("path")
                .datum(self.lineChartData)
                .attr("fill", "none")
                .attr("stroke", "steelblue")
                .attr("stroke-linejoin", "round")
                .attr("stroke-linecap", "round")
                .attr("stroke-width", 1.5)
                .attr("d", line);

        }
    }
});