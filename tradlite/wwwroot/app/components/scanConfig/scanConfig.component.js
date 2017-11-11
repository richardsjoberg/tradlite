tradliteApp.component("scanConfig", {
    templateUrl: "/app/components/scanConfig/scanConfig.html",
    controller: function ($scope, httpService) {
        $scope.clear_form = function () {
            $scope.model = {};
        }

        $scope.edit = function (scanConfig) {
            $scope.model = scanConfig;
        }
        
        $scope.save = function (scanConfig) {
            var promise;
            if (scanConfig.id) {
                promise = httpService.put("/api/scanConfig", scanConfig);
            } else {
                promise = httpService.post("/api/scanConfig", scanConfig);
            }
            promise.then(function () {
                getData();
            });
        }

        $scope.delete = function (scanConfig) {
            httpService.delete("/api/scanConfig/" + scanConfig.id).then(function () {
                getData();
            });
        }

        function getData() {
            httpService.get("/api/scanConfig").then(function (response) {
                $scope.scanConfigs = response.data;
            });
        }

        function init() {
            getData();
        }
        init();
    }
});