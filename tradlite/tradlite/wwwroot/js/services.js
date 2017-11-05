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

    function getKey(prefix, object) {
        var key = prefix;
        for (var property in object) {
            if (object.hasOwnProperty(property)) {
                key += "_" + object[property];
            }
        }
        return key;
    }

    return {
        setSessionStorage: setSessionStorage,
        getSessionStorage: getSessionStorage,
        setLocalStorage: setLocalStorage,
        getLocalStorage: getLocalStorage,
        getKey: getKey
    }
});