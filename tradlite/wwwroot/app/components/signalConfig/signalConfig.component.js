tradliteApp.component("signalConfig", {
    templateUrl: "/app/components/signalConfig/signalConfig.html",
    controller: function ($scope, httpService) {
        $scope.clear_form = function () {
            $scope.model = { type: 'buy' };
        }

        $scope.edit = function (signalConfig) {
            $scope.model = signalConfig;
        }
        
        $scope.save = function (signalConfig) {
            var promise;
            if (signalConfig.id) {
                promise = httpService.put("/api/signalconfig", signalConfig);
            } else {
                promise = httpService.post("/api/signalconfig", signalConfig);
            }
            promise.then(function () {
                getData();
            });
        }

        $scope.delete = function (signalConfig) {
            httpService.delete("/api/signalconfig/" + signalConfig.id).then(function () {
                getData();
            });
        }

        function getData() {
            httpService.get("/api/signalconfig").then(function (response) {
                $scope.signalConfigs = response.data;
            });
        }

        function init() {
            $scope.types = ['buy', 'sell'];
            getData();
        }
        init();
    }
});