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

namespace Tradlite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
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
                        default:
                            throw new KeyNotFoundException(); 
                    }
                };
                return accesor;
            });
            var connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\richard\source\repos\tradlite\tradlite\tradlite.mdf;Integrated Security=True;Connect Timeout=30";
            services.AddSingleton<ICandleService, CandleService>();
            services.AddSingleton<IHttpService, HttpService>();
            services.AddTransient<IDbConnection, SqlConnection>(factory=> 
            {
                return new SqlConnection(connectionString);
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
