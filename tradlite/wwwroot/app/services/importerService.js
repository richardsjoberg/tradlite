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