tradliteApp.service("httpService", function ($http, $q) {

    function get(url, params, inSequence) {
        if (inSequence) {
            return getInSequence(url, params);
        } else {
            return $http({
                url: url, method: "GET", params: params
            });
        }
    }

    var tasks = [];
    var executing = false;
    function getInSequence(url, params) {
        var q = $q.defer();
        tasks.push({ q: q, url: url, params: params });
        execute();
        return q.promise;
    }

    function execute() {
        if (!executing && tasks.length > 0) {
            executing = true;
            var task = tasks[0];
            $http({
                url: task.url, method: "GET", params: task.params
            }).then(function (response) {
                task.q.resolve(response);
                tasks.shift();
                executing = false;
                execute();
            }).catch(function (err) {
                task.q.reject(err);
                tasks.shift();
                executing = false;
                execute();
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
tradliteApp.service("importerService", function () {
    function get() {
        return [
            {
                name: "Yahoo",
                intervals: ["DAY", "WEEK", "MONTH"],
                tickerPlaceholder: "Symbol"
            },
            {
                name: "Google",
                intervals: ["SECOND", "MINUTE", "HOUR", "DAY"],
                tickerPlaceholder: "Market/Symbol"
            },
            {
                name: "Ig",
                intervals: ["MINUTE", "MINUTE_15", "MINUTE_30", "HOUR", "HOUR_4", "DAY", "WEEK", "MONTH"],
                tickerPlaceholder: "Epic"
            }
        ];
    }

    return {
        get: get
    }
});
tradliteApp.service("storageService", function () {
    function setSessionStorage(object, key) {
        store(object, key, sessionStorage);
    }
    
    function setLocalStorage(object, key) {
        store(object, key, localStorage);
    }

    function store(object, key, storage) {
        if (key) {
            if (object) {
                if (typeof object === 'object') {
                    storage.setItem(key, JSON.stringify(object));
                } else {
                    storage.setItem(key, object);
                }
            }
            else {
                storage.removeItem(key);
            }
        }
        
    }
    
    function getSessionStorage(key) {
        return get(key, sessionStorage);
    }

    function getLocalStorage(key) {
        return get(key, localStorage);
    }

    function get(key, storage) {
        var item = storage.getItem(key);
        var isJson = false;
        try {
            var obj = JSON.parse(item); 
            return obj;
        } catch (err) {
            return item;
        }
    }
    

    return {
        setSessionStorage: setSessionStorage,
        getSessionStorage: getSessionStorage,
        setLocalStorage: setLocalStorage,
        getLocalStorage: getLocalStorage
    }
});