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