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
            RegisterIgServices(serviceCollection, configuration);
            //setup our DI
            var serviceProvider = serviceCollection
                .AddLogging()
                .AddSingleton<ICandleService, CandleService>()
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
            //var candleService = serviceProvider.GetService<ICandleService>();
            //var candleDictionary = new Dictionary<string, List<IOhlcv>>();
            //foreach(var ticker in tickers)
            //{
            //    var candles = candleService.GetCandles(new Models.Requests.CandleRequest
            //    {
            //        FromDate = DateTime.Now.AddMinutes(-210 * 15),
            //        ToDate = DateTime.Now,
            //        Importer = "Ig",
            //        Interval = "MINUTE_15",
            //        Ticker = ticker
            //    }).Result;
            //    candleDictionary.Add(ticker, candles.ToList());

            //    //ToDo update last candle in dict when lightstream updates
            //    //create new candle if 15 minute mark passed 

            //}
            var client = igService.GetIgClient().Result;
            var authenticationDetails = igService.GetAuthenticationDetails().Result;
            var streamingClient = new IGStreamingApiClient();
            var igContext = client.GetConversationContext();
            var test = streamingClient.Connect(igConfig["username"], igContext.cst, igContext.xSecurityToken, igContext.apiKey, authenticationDetails.lightstreamerEndpoint);
            logger.LogInformation(test.ToString());
            var tableListener = new ChartCandleTableListerner();
            var handyListener = new HandyTableListenerAdapter();
            var marketListener = new MarketDetailsTableListerner();
            marketListener.Update += MarketListener_Update;
            tableListener.Update += TableListener_Update;
            streamingClient.SubscribeToChartCandleData(tickers, ChartScale.OneSecond, tableListener);
            Console.ReadLine();
            //streamingClient.Disconnect();

        }
        

        private static void MarketListener_Update(object sender, UpdateArgs<L1LsPriceData> e)
        {
            Console.WriteLine(e.UpdateData.Bid);
        }

        private static void TableListener_Update(object sender, UpdateArgs<ChartCandelData> e)
        {
            Console.WriteLine($"ticker {e.ItemName}, {e.UpdateData.UpdateTime}, bid close {e.UpdateData.Bid.Close}, offer close {e.UpdateData.Offer.Close}, last traded volume: {e.UpdateData.LastTradedVolume}");
            
        }

        private static void RegisterIgServices(IServiceCollection services, IConfigurationRoot configuration)
        {
            var igConfig = configuration.GetSection("Ig");
            string password;
            string apiKey;
            if (!string.IsNullOrEmpty(configuration["TRADLITE_IG_ENCRYPTION_KEY"]))
            {
                password = Cryptography.DecryptString(igConfig["password"], configuration["TRADLITE_IG_ENCRYPTION_KEY"]);
                apiKey = Cryptography.DecryptString(igConfig["apikey"], configuration["TRADLITE_IG_ENCRYPTION_KEY"]);
            }
            else
            {
                password = igConfig["password"];
                apiKey = igConfig["apikey"];
            }

            services.AddSingleton(factory =>
            {
                var logger = factory.GetService<ILogger<IgImporter>>();
                return new IgImporter(igConfig["environment"], igConfig["username"], password, apiKey, (message) => logger.LogInformation(message));
            });
            services.AddSingleton<IIgService, IgService>(factory =>
            {
                var logger = factory.GetService<ILogger<IgService>>();
                return new IgService(igConfig["environment"], igConfig["username"], password, apiKey, (message) => logger.LogInformation(message));
            });
        }
    }
}
