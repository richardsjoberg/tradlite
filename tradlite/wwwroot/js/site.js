// Write your JavaScript code.
var tradliteApp = angular.module("tradliteApp", ["ui.router"]);

tradliteApp.config(function ($stateProvider, $urlRouterProvider) {
    $urlRouterProvider.otherwise("/mainchart");
    $stateProvider.state("mainChart", {
        url: "/mainchart",
        component: "mainChart"
    })
        .state("signalConfig", {
            url: "/signalconfig",
            component: "signalConfig"
        })
        .state("ticker", {
            url: "/ticker",
            component: "ticker"
        })
        .state("tickerList", {
            url: "/tickerlist",
            component: "tickerList"
        })
        .state("browseTickers", {
            url: "/browsetickers",
            component: "browseTickers"
        })
        .state("scanConfig", {
            url: "/scanconfig",
            component: "scanConfig"
        })
        .state("scan", {
            url: "/scan",
            component: "scan"
        });
    

});