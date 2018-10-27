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