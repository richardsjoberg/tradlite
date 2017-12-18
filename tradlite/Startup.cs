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

            if (Configuration.GetValue<bool>("EnableIg"))
                RegisterIgServices(services);

            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes());
            RegisterServices<IStopLossManagement>(services, allTypes);
            RegisterServices<ILimitManagement>(services, allTypes);
            RegisterServices<IEntryManagement>(services, allTypes);
            RegisterServices<ISignalService>(services, allTypes);
            RegisterServices<IEntryFilterManagement>(services, allTypes);
        }

        private void RegisterServices<T>(IServiceCollection serviceCollection, IEnumerable<Type> allTypes)
        {
            var @interface = typeof(T);
            var services = allTypes.Where(p => @interface.IsAssignableFrom(p))
                .Except(new[] { @interface })
                .ToList();

            foreach (var service in services)
            {
                serviceCollection.Add(new ServiceDescriptor(service, service, ServiceLifetime.Transient));
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

        private void RegisterIgServices(IServiceCollection services)
        {
            var igConfig = Configuration.GetSection("Ig");
            string password;
            string apiKey;
            if (!string.IsNullOrEmpty(Configuration["TRADLITE_IG_ENCRYPTION_KEY"]))
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
