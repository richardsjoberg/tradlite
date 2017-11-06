tradliteApp.component("ticker", {
    templateUrl: "/app/components/ticker/ticker.html",
    controller: function ($scope, httpService, importerService) {
        $scope.clear_form = function () {
            $scope.model = { type: 'buy' };
        }

        $scope.edit = function (ticker) {
            $scope.model = ticker;
        }
        
        $scope.save = function (ticker) {
            var promise;
            if (ticker.id) {
                promise = httpService.put("/api/ticker", ticker);
            } else {
                promise = httpService.post("/api/ticker", ticker);
            }
            promise.then(function () {
                getData();
            });
        }

        $scope.delete = function (ticker) {
            httpService.delete("/api/ticker/" + ticker.id).then(function () {
                getData();
            });
        }

        function getData() {
            httpService.get("/api/ticker").then(function (response) {
                $scope.tickers = response.data;
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