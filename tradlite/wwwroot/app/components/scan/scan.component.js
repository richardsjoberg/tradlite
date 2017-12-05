tradliteApp.component("scan", {
    templateUrl: "/app/components/scan/scan.html",
    controller: function ($scope, $q, $state, httpService, storageService, importerService) {
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
            storageService.setSessionStorage($scope.importer.name, "importer");
            storageService.setSessionStorage($scope.tickerList.id, "tickerListId");

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

            if (storageService.getSessionStorage("importer")) {
                var importer = _.find($scope.importers, function (imp) { return imp.name === storageService.getSessionStorage("importer") });
                if (importer) {
                    $scope.importer = importer;
                }
            }
        }

        $scope.importer_changed = function (importer) {
            $scope.intervals = importer.intervals;
        }

        function getTickerLists() {
            httpService.get("/api/tickerList").then(function (response) {
                $scope.tickerLists = response.data;
                if (storageService.getSessionStorage("tickerListId")) {
                    var tickerList = _.find($scope.tickerLists, function (tl) { return tl.id === storageService.getSessionStorage("tickerListId") });
                    if (tickerList) {
                        $scope.tickerList = tickerList;
                    }
                } else {
                    $scope.tickerList = $scope.tickerLists[0];
                };
                getTickers($scope.tickerList.id);
            });
        }

        $scope.ticker_list_changed = function (tickerList) {
            getTickers(tickerList.id);
        }

        function getTickers(tickerListId) {
            httpService.get("/api/ticker/" + tickerListId).then(function (response) {
                $scope.tickers = response.data;
            });
        }
       
        $scope.scan = function () {
            $scope.signals = [];
            $scope.candles = undefined;
            $scope.ticker = undefined;
            $scope.buyIndicies = undefined;
            $scope.sellIndicies = undefined;
            angular.forEach($scope.tickers, function (ticker) {
                var request = { ticker: ticker.symbol, importer: ticker.importer, interval: $scope.interval, fromDate: $scope.fromDate, toDate: $scope.toDate }; 
                setSessionStorage();
                httpService.get("/api/candles", request, true).then(function (candleResponse) {
                    var candles = candleResponse.data;
                    if ($scope.buySignalConfig) {
                        if ($scope.buySignalConfig.parameters) {
                            var buySignalRequest = angular.copy(request);
                            buySignalRequest.parameters = $scope.buySignalConfig.parameters
                        }
                        httpService.get($scope.buySignalConfig.endpoint, buySignalRequest, true).then(function (buyResponse) {
                            var buyIndicies = buyResponse.data;
                            if (candles.length - 2 < buyIndicies[buyIndicies.length - 1]) {
                                $scope.signals.push({
                                    ticker: ticker,
                                    type: "buy",
                                    candles: candles,
                                    buyIndicies: buyIndicies,
                                    sellIndicies: []
                                });
                            }
                            console.log(buyResponse.data);
                        });
                    }
                    if ($scope.sellSignalConfig) {
                        if ($scope.sellSignalConfig.parameters) {
                            var sellSignalRequest = angular.copy(request);
                            sellSignalRequest.parameters = $scope.sellSignalConfig.parameters
                        }
                        httpService.get($scope.sellSignalConfig.endpoint, sellSignalRequest, true).then(function (sellResponse) {
                            var sellIndicies = sellResponse.data;
                            if (candles.length - 2 < sellIndicies[sellIndicies.length - 1]) {
                                $scope.signals.push({
                                    ticker: ticker,
                                    type: "sell",
                                    candles: candles,
                                    sellIndicies: sellIndicies,
                                    buyIndicies: []
                                });
                            }
                            console.log(sellResponse.data);
                        });
                    }
                }).catch(console.log);
            });
        }

        $scope.navigate_to_chart = function (signal) {
            $state.go('mainChart', { ticker: signal.ticker.symbol });
        }

        $scope.view_chart = function (signal) {
            $scope.candles = signal.candles;
            $scope.ticker = signal.ticker;
            $scope.buyIndicies = signal.buyIndicies;
            $scope.sellIndicies = signal.sellIndicies;
            $scope.tickerLabel = signal.ticker.name + ', ' + signal.ticker.symbol;
        } 

        $scope.next_signal = function () {
            var currentSignalIndex = 0;
            var nextSignalIndex = 0;
            angular.forEach($scope.signals, function (s,index) {
                if (s.ticker.symbol === $scope.ticker.symbol) {
                    currentSignalIndex = index 
                }
            });

            if ($scope.signals.length - 1 > currentSignalIndex) {
                nextSignalIndex = currentSignalIndex + 1;
            } else {
                nextSignalIndex = 0;
            }

            if (currentSignalIndex > 0) {
                signalIndex = nextSignalIndex - 1;
            } else {
                signalIndex = nextSignalIndex + 1;
            }

            $scope.view_chart($scope.signals[nextSignalIndex]);
        }

        $scope.previous_signal = function () {
            var currentSignalIndex = 0;
            var nextSignalIndex = 0;
            angular.forEach($scope.signals, function (s, index) {
                if (s.ticker.symbol === $scope.ticker.symbol) {
                    currentSignalIndex = index
                }
            });

            if (currentSignalIndex > 0) {
                signalIndex = nextSignalIndex - 1;
            } else {
                signalIndex = nextSignalIndex + 1;
            }
            console.log(nextSignalIndex);
            $scope.view_chart($scope.signals[nextSignalIndex]);
        }

        this.$onInit = function () {
            $scope.importers = importerService.get();
            $scope.interval = "DAY";
            $scope.importer = $scope.importers[0];
            $scope.importer_changed($scope.importers[0]);
            getSignalConfigs();
            getDataFromSessionStorage();
            getTickerLists();
        }
    }
});