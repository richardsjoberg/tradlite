tradliteApp.service("httpService", function ($http) {

    function get(url, params) {
        return $http({
            url: url, method: "GET", params: params
        })
    }

    function post(url, body) {
        return $http({ url: url, method: "POST", data: body })
    }

    function put(url, body) {
        return $http({ url: url, method: "PUT", data: body })
    }

    function del(url) {
        return $http({ url, method: "DELETE" });
    }

    return {
        get: get,
        post: post,
        put: put,
        delete: del
    }
});