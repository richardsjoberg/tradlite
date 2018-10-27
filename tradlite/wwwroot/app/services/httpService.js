tradliteApp.service("httpService", function ($http, $q) {

    function get(url, params, inSequence, delay) {
        if (inSequence) {
            return getInSequence(url, params, delay);
        } else {
            return $http({
                url: url, method: "GET", params: params
            });
        }
    }

    var tasks = [];
    var executing = false;
    function getInSequence(url, params, delay) {
        var q = $q.defer();
        tasks.push({ q: q, url: url, params: params });
        execute(delay);
        return q.promise;
    }

    function execute(delay) {
        if (!executing && tasks.length > 0) {
            executing = true;
            var task = tasks[0];
            if (!delay)
                delay = 0;

            $http({ url: task.url, method: "GET", params: task.params })
                .then(function (response) {
                    setTimeout(function () {
                        task.q.resolve(response);
                        tasks.shift();
                        executing = false;
                        execute(delay);
                    }, delay);
                }).catch(function (err) {
                    setTimeout(function () {
                        task.q.reject(err);
                        tasks.shift();
                        executing = false;
                        execute(delay);
                    }, delay);
                });
        }
    }
    
    function post(url, body) {
        return $http({ url: url, method: "POST", data: body });
    }

    function put(url, body) {
        return $http({ url: url, method: "PUT", data: body });
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