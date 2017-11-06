tradliteApp.component("tickerList", {
    templateUrl: "/app/components/tickerList/tickerList.html",
    controller: function ($scope, httpService, importerService) {
        $scope.clear_form = function () {
            $scope.model = { type: 'buy' };
        }

        $scope.edit = function (tickerList) {
            $scope.model = tickerList;
            httpService.get("/api/ticker/" + tickerList.id).then(function (response) {
                $scope.tickerListTickers = response.data;
            });
        }
        
        $scope.save = function (tickerList) {
            var promise;
            if (tickerList.id) {
                promise = httpService.put("/api/tickerList", tickerList);
            } else {
                promise = httpService.post("/api/tickerList", tickerList);
            }
            promise.then(function () {
                getData();
            });
        }

        $scope.delete = function (tickerList) {
            httpService.delete("/api/tickerList/" + tickerList.id).then(function () {
                getData();
            });
        }

        $scope.add_ticker = function (ticker) {
            httpService.post("/api/tickerlist/addticker/" + $scope.model.id + "/" + ticker.id).then(function () {
                $scope.edit($scope.model);
            });
        }

        $scope.remove_ticker = function (ticker) {
            httpService.post("api/tickerlist/removeticker/" + $scope.model.id + "/" + ticker.id).then(function () {
                $scope.edit($scope.model);
            });
        }

        function getData() {
            httpService.get("/api/tickerList").then(function (response) {
                $scope.tickerLists = response.data;
            });
            httpService.get("/api/ticker").then(function (response) {
                $scope.tickers = response.data;
                $scope.ticker = response.data[0];
            });
        }

        function init() {
            $scope.model = {};
            $scope.importers = importerService.get();
            $scope.model.importer = $scope.importers[0].name;
            getData();
        }
        init();
    }
});