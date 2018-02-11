using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Tradlite.Services;
using Tradlite.Services.Backtest;
using Tradlite.Services.Candle.CandleService;
using Tradlite.Services.Ig;
using Tradlite.Services.Management;
using Tradlite.Services.Signals;
using Trady.Core.Infrastructure;
using Trady.Importer;
using Trady.Importer.Csv;
using Trady.Importer.Google;
using Trady.Importer.Quandl;
using Trady.Importer.Stooq;
using Trady.Importer.Yahoo;

namespace Tradlite
{
    public static class TradliteRegistry
    {
        public static void ConfigureTradliteServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ICandleService, CandleService>();
            services.AddSingleton<IHttpService, HttpService>();
            services.AddSingleton<IBacktestService, BacktestService>();

            //ToDo: remove this and use connectionFactory instead
            services.AddTransient<IDbConnection, SqlConnection>(factory =>
            {
                return new SqlConnection(configuration.GetConnectionString("tradlite"));
            });

            services.AddTransient(factory =>
            {
                Func<IDbConnection> connectionFactory = () =>
                {
                    return new SqlConnection(configuration.GetConnectionString("tradlite"));
                };

                return connectionFactory;
            });

            services.AddTransient<YahooFinanceImporter>();
            services.AddTransient<GoogleFinanceImporter>();
            services.AddTransient<QuandlImporter>();
            services.AddTransient<StooqImporter>();
            services.AddTransient<CsvImporter>();
            services.AddTransient(factory =>
            {
                Func<string, IImporter> accesor = key =>
                {
                    switch (key)
                    {
                        case "Yahoo":
                            return factory.GetService<YahooFinanceImporter>();
                        case "Google":
                            return factory.GetService<GoogleFinanceImporter>();
                        case "Quandl":
                            return factory.GetService<QuandlImporter>();
                        case "Stooq":
                            return factory.GetService<StooqImporter>();
                        case "Csv":
                            return factory.GetService<CsvImporter>();
                        case "Ig":
                            return factory.GetService<IgImporter>();
                        default:
                            throw new KeyNotFoundException();
                    }
                };
                return accesor;
            });

            if (configuration.GetValue<bool>("EnableIg"))
                RegisterIgServices(services, configuration);

            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes());
            RegisterServices<IStopLossManagement>(services, allTypes);
            RegisterServices<ILimitManagement>(services, allTypes);
            RegisterServices<IEntryManagement>(services, allTypes);
            RegisterServices<ISignalService>(services, allTypes);
            RegisterServices<IEntryFilterManagement>(services, allTypes);
        }


        private static void RegisterServices<T>(IServiceCollection serviceCollection, IEnumerable<Type> allTypes)
        {
            var @interface = typeof(T);
            var services = allTypes.Where(p => @interface.IsAssignableFrom(p))
                .Except(new[] { @interface })
                .ToList();

            foreach (var service in services)
            {
                serviceCollection.Add(new ServiceDescriptor(service, service, ServiceLifetime.Singleton));
            }

            serviceCollection.AddTransient(factory =>
            {
                Func<string, T> accessor = key =>
                {
                    var signalType = services.FirstOrDefault(s => s.Name.ToLower() == key.ToLower());
                    if (signalType == null)
                        throw new KeyNotFoundException();

                    return (T)factory.GetService(signalType);
                };
                return accessor;
            });
        }

        private static void RegisterIgServices(IServiceCollection services, IConfiguration configuration)
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
