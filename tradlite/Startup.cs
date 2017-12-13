using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tradlite.Services.Candle.CandleService;
using Trady.Core.Infrastructure;
using Trady.Importer;
using System.Data;
using System.Data.SqlClient;
using Tradlite.Services;
using Trady.Importer.Stooq;
using Trady.Importer.Quandl;
using Trady.Importer.Google;
using Trady.Importer.Yahoo;
using Trady.Importer.Csv;
using Tradlite.Services.Signals;
using Tradlite.Services.Ig;
using Microsoft.Extensions.Logging;
using Tradlite.Services.Management;
using Tradlite.Services.Signals.DirectionalMovement;
using Tradlite.Services.Backtest;

namespace Tradlite
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
            Configuration = builder.Build();

            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddLogging();
            
            services.AddTransient<ICandleService, CandleService>();
            services.AddSingleton<IHttpService, HttpService>();
            services.AddSingleton<IDirectionalMovementService, DirectionalMovementService>();
            services.AddSingleton<IRsiService, RsiService>();
            services.AddSingleton<ICandlePatternService, CandlePatternService>();
            services.AddSingleton<IMovingAverageService, MovingAverageService>();
            services.AddSingleton<IZigZagService, ZigZagService>();
            services.AddSingleton<IBacktestService, BacktestService>();

            //ToDo: remove this and use connectionFactory instead
            services.AddTransient<IDbConnection, SqlConnection>(factory =>
            {
                return new SqlConnection(Configuration.GetConnectionString("tradlite"));
            });
            
            services.AddTransient(factory =>
            {
                Func<IDbConnection> connectionFactory = () =>
                {
                    return new SqlConnection(Configuration.GetConnectionString("tradlite"));
                };

                return connectionFactory;
            });

            if (Configuration.GetValue<bool>("EnableIg"))
            {
                var igConfig = Configuration.GetSection("Ig");
                string password;
                string apiKey;
                if(!string.IsNullOrEmpty(Configuration["TRADLITE_IG_ENCRYPTION_KEY"]))
                {
                    password = Cryptography.DecryptString(igConfig["password"], Configuration["TRADLITE_IG_ENCRYPTION_KEY"]);
                    apiKey = Cryptography.DecryptString(igConfig["apikey"], Configuration["TRADLITE_IG_ENCRYPTION_KEY"]);
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

            services.AddTransient<AverageTrueRangeLongStopLoss>();
            services.AddTransient<AverageTrueRangeShortStopLoss>();
            services.AddTransient<CurrentLowStopLoss>();
            services.AddTransient(factory =>
            {
                Func<string, IStopLossManagement> accesor = key =>
                {
                    switch (key)
                    {
                        case "AverageTrueRangeLongStopLoss":
                            return factory.GetService<AverageTrueRangeLongStopLoss>();
                        case "AverageTrueRangeShortStopLoss":
                            return factory.GetService<AverageTrueRangeShortStopLoss>();
                        case "CurrentLowStopLoss":
                            return factory.GetService<CurrentLowStopLoss>();
                        default:
                            throw new KeyNotFoundException();
                    }
                };
                return accesor;
            });

            services.AddTransient<AverageTrueRangeLongLimit>();
            services.AddTransient<AverageTrueRangeShortLimit>();
            services.AddTransient(factory =>
            {
                Func<string, ILimitManagement> accesor = key =>
                {
                    switch (key)
                    {
                        case "AverageTrueRangeLongLimit":
                            return factory.GetService<AverageTrueRangeLongLimit>();
                        case "AverageTrueRangeShortLimit":
                            return factory.GetService<AverageTrueRangeShortLimit>();
                        default:
                            throw new KeyNotFoundException();
                    }
                };
                return accesor;
            });

            services.AddTransient<CurrentCloseEntry>();
            services.AddTransient<CurrentCloseVwapLongEntry>();
            services.AddTransient<CurrentCloseVwapShortEntry>();
            services.AddTransient(factory =>
            {
                Func<string, IEntryManagement> accessor = key =>
                {
                    switch (key)
                    {
                        case "CurrentCloseEntry":
                            return factory.GetService<CurrentCloseEntry>();
                        case "CurrentCloseVwapLongEntry":
                            return factory.GetService<CurrentCloseVwapLongEntry>();
                        case "CurrentCloseVwapShortEntry":
                            return factory.GetService<CurrentCloseVwapShortEntry>();
                        default:
                            throw new KeyNotFoundException();
                    }
                };
                return accessor;
            });
            
            services.AddTransient<StandardDeviationRatioEntryFilter>();
            services.AddTransient<RsiOverboughtEntryFilter>();
            services.AddTransient<RsiOversoldEntryFilter>();
            services.AddTransient(factory =>
            {
                Func<string, IEntryFilterManagement> accessor = key =>
                {
                    switch (key)
                    {
                        case "StandardDeviationRatioEntryFilter":
                            return factory.GetService<StandardDeviationRatioEntryFilter>();
                        case "RsiOverboughtEntryFilter":
                            return factory.GetService<RsiOverboughtEntryFilter>();
                        case "RsiOversoldEntryFilter":
                            return factory.GetService<RsiOversoldEntryFilter>();
                        default:
                            throw new KeyNotFoundException();
                    }
                };
                return accessor;
            });

            services.AddTransient<TrendService>();
            services.AddTransient<NoTrendService>();
            services.AddTransient(factory =>
            {
                Func<string, ISignalService> accessor = key =>
                {
                    switch (key)
                    {
                        case "directionalmovementtrend":
                            return factory.GetService<TrendService>();
                        case "directionalmovementnotrend":
                            return factory.GetService<NoTrendService>();
                        default:
                            throw new KeyNotFoundException();
                    }
                };
                return accessor;
            });

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
