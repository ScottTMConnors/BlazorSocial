window.localStorageManager = {
    setItem: function (key, value) {
        try {
            localStorage.setItem(key, value);
            this.updateLRUList(key);
        } catch (e) {
            if (e.name === 'QuotaExceededError') {
                this.evictItems();
                localStorage.setItem(key, value);
                this.updateLRUList(key);
            }
        }
    },
    getItem: function (key) {
        let item = localStorage.getItem(key);
        if (item) {
            this.updateLRUList(key);
        }
        return item;
    },
    removeItem: function (key) {
        localStorage.removeItem(key);
    },
    lruList: [],
    updateLRUList: function (key) {
        let index = this.lruList.indexOf(key);
        if (index > -1) {
            this.lruList.splice(index, 1);
        }
        this.lruList.push(key);
    },
    evictItems: function () {
        while (this.lruList.length > 0) {
            let keyToRemove = this.lruList.shift();
            this.removeItem(keyToRemove);
            try {
                localStorage.setItem('test', 'test');
                localStorage.removeItem('test');
                break;
            } catch (e) {
                continue;
            }
        }
    }
};
