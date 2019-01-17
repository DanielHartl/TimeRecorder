using ActivityTracker.Server.Domain;
using ActivityTracker.Server.Persistence;
using ActivityTracker.Server.Persistence.AzureStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using ActivityTracker.Server.App.Formatter;

namespace ActivityTracker.Server.App
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            HostingEnvironment = env;
            Configuration = configuration;
        }

        public IHostingEnvironment HostingEnvironment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            IActivityRepository activityRepository;

            if (HostingEnvironment.IsDevelopment())
            {
                activityRepository = new InMemoryActivityRepository();
            }
            else
            {
                var storageCredentialsSection = Configuration.GetSection("StorageCredentials");
                var storageCredentials = new StorageCredentials(
                    storageCredentialsSection["Name"],
                    storageCredentialsSection["Key"]);

                activityRepository = new TableActivityRepository(storageCredentials);
            }

            var toleranceWindow = TimeSpan.FromMinutes(5);

            services.AddTransient<IActivityService>(si =>
                new ActivityService(activityRepository, toleranceWindow));

            services.AddTransient<IActivitySummaryFormatter>(si =>
                new DefaultActivitySummaryFormatter());

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa => {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}