tradliteApp.component("igBrowser", {
    templateUrl: "/app/components/igBrowser/igBrowser.html",
    controller: function ($scope, $q, $state, httpService, storageService, importerService, $stateParams) {
        //$scope.load_chart = function () {
        //    var request = { ticker: $scope.ticker, fromDate: $scope.fromDate, toDate: $scope.toDate, importer: $scope.importer.name, interval: $scope.interval };
        //    setSessionStorage();
        //    httpService.get("/api/candles", request).then(function (candleResponse) {
        //        $scope.candles = candleResponse.data;
        //        if ($scope.buySignalConfig) {
        //            if ($scope.buySignalConfig.extraParams) {
        //                var buySignalRequest = angular.copy(request);
        //                buySignalRequest.extraParams = $scope.buySignalConfig.extraParams
        //            }
        //            httpService.get($scope.buySignalConfig.endpoint, buySignalRequest).then(function (buyResponse) {
        //                $scope.buyIndicies = buyResponse.data;
        //            });
        //        }
        //        if ($scope.sellSignalConfig) {
        //            if ($scope.sellSignalConfig.extraParams) {
        //                var sellSignalRequest = angular.copy(request);
        //                sellSignalRequest.extraParams = $scope.sellSignalConfig.extraParams
        //            }
        //            httpService.get($scope.sellSignalConfig.endpoint, sellSignalRequest).then(function (sellResponse) {
        //                $scope.sellIndicies = sellResponse.data;
        //            });
        //        }
        //    });
        //}

        //function getSignalConfigs() {
        //    var promise1 = httpService.get("/api/signalconfig/buy");
        //    var promise2 = httpService.get("/api/signalconfig/sell");
        //    return $q.all([promise1, promise2]).then(function (responses) {
        //        $scope.buySignalConfigs = responses[0].data;
        //        $scope.sellSignalConfigs = responses[1].data;

        //        if (storageService.getSessionStorage("buySignalConfig")) {
        //            var buySignalConfig = _.find($scope.buySignalConfigs, function (conf) { return conf.id === storageService.getSessionStorage("buySignalConfig") });
        //            if (buySignalConfig) {
        //                $scope.buySignalConfig = buySignalConfig;
        //            }
        //        }
        //        if (storageService.getSessionStorage("sellSignalConfig")) {
        //            var sellSignalConfig = _.find($scope.sellSignalConfigs, function (conf) { return conf.id === storageService.getSessionStorage("sellSignalConfig") });
        //            if (sellSignalConfig) {
        //                $scope.sellSignalConfig = sellSignalConfig;
        //            }
        //        }
        //    });
        //}

        //function setSessionStorage() {
        //    storageService.setSessionStorage($scope.ticker, "ticker");
        //    storageService.setSessionStorage($scope.fromDate, "fromDate");
        //    storageService.setSessionStorage($scope.toDate, "toDate");
        //    storageService.setSessionStorage($scope.importer.name, "importer");

        //    if ($scope.buySignalConfig)
        //        storageService.setSessionStorage($scope.buySignalConfig.id, "buySignalConfig");
        //    else
        //        storageService.setSessionStorage(undefined, "buySignalConfig");

        //    if ($scope.sellSignalConfig)
        //        storageService.setSessionStorage($scope.sellSignalConfig.id, "sellSignalConfig");
        //    else
        //        storageService.setSessionStorage(undefined, "sellSignalConfig");
        //}

        //function getDataFromSessionStorage() {
        //    if (storageService.getSessionStorage("ticker"))
        //        $scope.ticker = storageService.getSessionStorage("ticker");

        //    if (storageService.getSessionStorage("fromDate"))
        //        $scope.fromDate = storageService.getSessionStorage("fromDate");

        //    if (storageService.getSessionStorage("toDate"))
        //        $scope.toDate = storageService.getSessionStorage("toDate");

        //    if (storageService.getSessionStorage("importer")) {
        //        var importer = _.find($scope.importers, function (imp) { return imp.name === storageService.getSessionStorage("importer") });
        //        if (importer) {
        //            $scope.importer = importer;
        //        }
        //    }
        //}

        //$scope.importer_changed = function (importer) {
        //    $scope.tickerPlaceholder = importer.tickerPlaceholder;
        //    $scope.intervals = importer.intervals;
        //}
        $scope.browse = function (nodeId) {
            $state.transitionTo('igBrowser', { nodeId: nodeId }, {
                location: true,
                inherit: true,
                relative: $state.$current,
                notify: true
            });
            
        }
        
        this.$onInit = function () {
            console.log($state.params.nodeId);
            httpService.get("/api/ig/browse", { nodeId: $state.params.nodeId }).then(function (response) {
                $scope.nodes = response.data.nodes;
                console.log($scope.nodes);
                $scope.markets = response.data.markets;
            });
        }
    }
});