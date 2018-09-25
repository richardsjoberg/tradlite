using IGWebApiClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Topshelf;
using Tradlite.Services.Candle.CandleService;
using Tradlite.Services.Ig;
using Trady.Core.Infrastructure;
using Trady.Importer;

namespace Tradlite.IgPocBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();
            
            IConfigurationRoot configuration = builder.Build();

            var igConfig = configuration.GetSection("Ig");

            var serviceCollection = new ServiceCollection();
            TradliteRegistry.ConfigureTradliteServices(serviceCollection, configuration);
            //setup our DI
            var serviceProvider = serviceCollection
                .AddLogging()
                .BuildServiceProvider();
           
            //configure console logging
            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            
            logger.LogInformation("Starting application");
            var tickers = new[] { "CS.D.ETHUSD.CFD.IP", "CS.D.BITCOIN.CFD.IP" };
            var igService = serviceProvider.GetService<IIgService>();
            var candleService = serviceProvider.GetService<ICandleService>();
            var candleDictionary = new Dictionary<string, List<IOhlcv>>();
            foreach (var ticker in tickers)
            {
                var candles = candleService.GetCandles(new Models.Requests.CandleRequest
                {
                    FromDate = DateTime.Now.AddMinutes(-210 * 15),
                    ToDate = DateTime.Now,
                    Importer = "Ig",
                    Interval = "MINUTE_15",
                    Ticker = ticker
                }).Result;
                candleDictionary.Add(ticker, candles.ToList());

                //ToDo update last candle in dict when lightstream updates
                //create new candle if 15 minute mark passed 

            }
            var candlesUpdater = new CandleUpdater(igService);
            var engine = new TradeEngine(candlesUpdater, candleDictionary[tickers[0]], tickers[0]);
            candlesUpdater.Initialize(tickers, igConfig["username"], candleDictionary.ToDictionary(cd => cd.Key, cd => cd.Value.Last().DateTime));
            Console.ReadLine();
            //streamingClient.Disconnect();
            
        }
        

        
    }
}
