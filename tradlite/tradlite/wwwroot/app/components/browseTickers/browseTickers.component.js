tradliteApp.component("browseTickers", {
    templateUrl: "/app/components/browseTickers/browseTickers.html",
    controller: function ($scope, $q, httpService, storageService, importerService) {
        function getSignalConfigs() {
            var promise1 = httpService.get("/api/signalconfig/buy");
            var promise2 = httpService.get("/api/signalconfig/sell");
            $q.all([promise1, promise2]).then(function (responses) {
                $scope.buySignalConfigs = responses[0].data;
                $scope.sellSignalConfigs = responses[1].data;

                if (storageService.getSessionStorage("buySignalConfig")) {
                    var buySignalConfig = _.find($scope.buySignalConfigs, function (conf) { return conf.id === storageService.getSessionStorage("buySignalConfig") });
                    if (buySignalConfig) {
                        $scope.buySignalConfig = buySignalConfig;
                    }
                }
                if (storageService.getSessionStorage("sellSignalConfig")) {
                    var sellSignalConfig = _.find($scope.sellSignalConfigs, function (conf) { return conf.id === storageService.getSessionStorage("sellSignalConfig") });
                    if (sellSignalConfig) {
                        $scope.sellSignalConfig = sellSignalConfig;
                    }
                }
            });
        }

        function setSessionStorage() {
            storageService.setSessionStorage($scope.currentTickerIndex, "currentTickerIndex");
            storageService.setSessionStorage($scope.fromDate, "fromDate");
            storageService.setSessionStorage($scope.toDate, "toDate");
            storageService.setSessionStorage($scope.interval, "interval");
            if ($scope.buySignalConfig)
                storageService.setSessionStorage($scope.buySignalConfig.id, "buySignalConfig");
            else
                storageService.setSessionStorage(undefined, "buySignalConfig");

            if ($scope.sellSignalConfig)
                storageService.setSessionStorage($scope.sellSignalConfig.id, "sellSignalConfig");
            else
                storageService.setSessionStorage(undefined, "sellSignalConfig");
        }

        function getDataFromSessionStorage() {
            if (storageService.getSessionStorage("fromDate"))
                $scope.fromDate = storageService.getSessionStorage("fromDate");

            if (storageService.getSessionStorage("toDate"))
                $scope.toDate = storageService.getSessionStorage("toDate");
        }

        $scope.importer_changed = function (importer) {
            $scope.intervals = importer.intervals;
        }

        function getTickerLists() {
            httpService.get("/api/tickerList").then(function (response) {
                $scope.tickerLists = response.data;
                $scope.tickerList = $scope.tickerLists[0];
                $scope.ticker_list_changed($scope.tickerLists[0]);
            });
        }

        $scope.ticker_list_changed = function (tickerList) {
            getTickers(tickerList.id);
        }

        function getTickers(tickerListId) {
            httpService.get("/api/ticker/" + tickerListId).then(function (response) {
                $scope.tickers = response.data;
                $scope.currentTickerIndex = 0;
                $scope.load_chart();
            });
        }

        $scope.next_ticker = function () {
            if ($scope.tickers.length - 1 > $scope.currentTickerIndex)
            {
                $scope.currentTickerIndex++;
            } else {
                $scope.currentTickerIndex = 0;
            }

            $scope.load_chart();
        }

        $scope.previous_ticker = function () {
            if ($scope.currentTickerIndex > 0) {
                $scope.currentTickerIndex--;
            } else {
                $scope.currentTickerIndex++;
            }

            $scope.load_chart();
        }

        $scope.load_chart = function () {
            var ticker = $scope.tickers[$scope.currentTickerIndex];
            var request = { ticker: ticker.symbol, fromDate: $scope.fromDate, toDate: $scope.toDate, importer: ticker.importer, interval: $scope.interval };
            setSessionStorage();
            httpService.get("/api/candles", request).then(function (candleResponse) {
                $scope.candles = candleResponse.data;
                if ($scope.buySignalConfig) {
                    if ($scope.buySignalConfig.extraParams) {
                        var buySignalRequest = angular.copy(request);
                        buySignalRequest.extraParams = $scope.buySignalConfig.extraParams
                    }
                    httpService.get($scope.buySignalConfig.endpoint, buySignalRequest).then(function (buyResponse) {
                        $scope.buyIndicies = buyResponse.data;
                        console.log(buyResponse.data);
                    });
                }
                if ($scope.sellSignalConfig) {
                    if ($scope.sellSignalConfig.extraParams) {
                        var sellSignalRequest = angular.copy(request);
                        sellSignalRequest.extraParams = $scope.sellSignalConfig.extraParams
                    }
                    httpService.get($scope.sellSignalConfig.endpoint, sellSignalRequest).then(function (sellResponse) {
                        $scope.sellIndicies = sellResponse.data;
                        console.log(sellResponse.data);
                    });
                }
            });
        }

        this.$onInit = function () {
            $scope.importers = importerService.get();
            $scope.interval = "DAY";
            $scope.importer = $scope.importers[0];
            $scope.importer_changed($scope.importers[0]);
            $scope.buyIndicies = [];
            $scope.sellIndicies = [];
            getSignalConfigs();
            getDataFromSessionStorage();
            getTickerLists();
        }
    }
});