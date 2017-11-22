tradliteApp.component("igBrowser", {
    templateUrl: "/app/components/igBrowser/igBrowser.html",
    controller: function ($scope, $q, $state, httpService, storageService, importerService, $stateParams) {

        $scope.browse = function (nodeId) {
            $state.transitionTo('igBrowser', { nodeId: nodeId }, {
                location: true,
                inherit: true,
                relative: $state.$current,
                notify: true
            });
            
        }
        
        this.$onInit = function () {
            httpService.get("/api/ig/browse", { nodeId: $state.params.nodeId }).then(function (response) {
                $scope.nodes = response.data.nodes;
                $scope.markets = response.data.markets;
            });
        }
    }
});