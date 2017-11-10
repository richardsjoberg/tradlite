tradliteApp.component("mainChart", {
    templateUrl: "/app/components/mainChart/mainChart.html",
    controller: function ($scope, $q, httpService, storageService, importerService) {
        $scope.load_chart = function () {
            var request = { ticker: $scope.ticker, fromDate: $scope.fromDate, toDate: $scope.toDate, importer: $scope.importer.name, interval: $scope.interval };
            setSessionStorage();
            httpService.get("/api/candles", request).then(function (candleResponse) {
                $scope.candles = candleResponse.data;
                if ($scope.buySignalConfig) {
                    if ($scope.buySignalConfig.extraParams) {
                        request.extraParams = $scope.buySignalConfig.extraParams
                    }
                    httpService.get($scope.buySignalConfig.endpoint, request).then(function (buyResponse) {
                        $scope.buyIndicies = buyResponse.data;
                    });
                }
                if ($scope.sellSignalConfig) {
                    if ($scope.sellSignalConfig.extraParams) {
                        request.extraParams = $scope.sellSignalConfig.extraParams
                    }
                    httpService.get($scope.sellSignalConfig.endpoint, request).then(function (sellResponse) {
                        $scope.sellIndicies = sellResponse.data;
                    });
                }
            });
        }

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
            storageService.setSessionStorage($scope.ticker, "ticker");
            storageService.setSessionStorage($scope.fromDate, "fromDate");
            storageService.setSessionStorage($scope.toDate, "toDate");
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
            if (storageService.getSessionStorage("ticker"))
                $scope.ticker = storageService.getSessionStorage("ticker");

            if (storageService.getSessionStorage("fromDate"))
                $scope.fromDate = storageService.getSessionStorage("fromDate");

            if (storageService.getSessionStorage("toDate"))
                $scope.toDate = storageService.getSessionStorage("toDate");
        }

        $scope.importer_changed = function (importer) {
            $scope.tickerPlaceholder = importer.tickerPlaceholder;
            $scope.intervals = importer.intervals;
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
        }
    }
});