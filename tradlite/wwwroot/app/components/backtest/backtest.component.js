tradliteApp.component("backtest", {
    templateUrl: "/app/components/backtest/backtest.html",
    controller: function ($scope, $q, $state, httpService) {
        $scope.execute_test = function () {
            var request = {
                tickerListId: $scope.tickerListId,
                longBacktestConfigId: $scope.longBacktestConfigId,
                shortBacktestConfigId: $scope.shortBacktestConfigId,
                interval: $scope.interval,
                risk: $scope.risk,
                fromDate: $scope.fromDate,
                toDate: $scope.toDate
            };
            httpService.get("/api/backtest/tickerlist", request).then(function (response) {
                $scope.transactions = angular.copy(response.data.transactions);
                
                delete response.data.transactions;
                $scope.backtestResult = response.data;
                var capital = 100000;
                $scope.lineChartData = [];
                angular.forEach($scope.transactions, function (transaction) {
                    capital = capital + transaction.gain;
                    $scope.lineChartData.push({
                        yValue: capital,
                        date: d3.timeParse("%Y-%m-%dT%H:%M:%S")(transaction.entryDate.substring(0, transaction.entryDate.indexOf('+')))
                    });
                });
            });
        }

        this.$onInit = function () {

        }
    }
});