tradliteApp.component("volatilitySpreadRatio", {
    templateUrl: "/app/components/volatilitySpreadRatio/volatilitySpreadRatio.html",
    controller: function ($scope, $q, $state, httpService, storageService, importerService) {
        
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

        function setSessionStorage() {
            storageService.setSessionStorage($scope.fromDate, "fromDate");
            storageService.setSessionStorage($scope.toDate, "toDate");
            storageService.setSessionStorage($scope.interval, "interval");
            storageService.setSessionStorage($scope.tickerList.id, "tickerListId");
        }

        function getDataFromSessionStorage() {
            if (storageService.getSessionStorage("fromDate"))
                $scope.fromDate = storageService.getSessionStorage("fromDate");

            if (storageService.getSessionStorage("toDate"))
                $scope.toDate = storageService.getSessionStorage("toDate");

            if (storageService.getSessionStorage("interval"))
                $scope.interval = storageService.getSessionStorage("interval");
        }

        function getTickers(tickerListId) {
            httpService.get("/api/ticker/" + tickerListId).then(function (response) {
                $scope.tickers = response.data;
            });
        }
       
        $scope.run = function () {
            setSessionStorage();
            $scope.volatilitySpreadRatios = [];
            angular.forEach($scope.tickers, function (ticker) {
                var request = { ticker: ticker.symbol, importer: ticker.importer, interval: $scope.interval, fromDate: $scope.fromDate, toDate: $scope.toDate }; 
                httpService.get("/api/volatilityspreadratio", request, true, 2100).then(function (response) {
                    $scope.volatilitySpreadRatios.push({
                        ticker: ticker,
                        ratio: response.data.ratio,
                        spread: response.data.spread,
                        atr: response.data.atr
                    });
                }).catch(console.log);
            });
        }
        

        this.$onInit = function () {
            var importers = importerService.get();
            var igImporter = _.find(importers, function (imp) { return imp.name === "Ig" });
            $scope.intervals = igImporter.intervals;
            $scope.interval = "MINUTE";
            $scope.fromDate = moment().subtract(20, 'minutes').format("YYYY-MM-DDTHH:mm:ss");
            getDataFromSessionStorage();
            getTickerLists();
        }
    }
});