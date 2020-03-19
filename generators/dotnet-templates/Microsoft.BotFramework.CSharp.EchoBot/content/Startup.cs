// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version __vX.X.X__

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
#if (!UseCore31)
using Microsoft.AspNetCore.Mvc;
#endif
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#if (UseCore31)
using Microsoft.Extensions.Hosting;
#endif

using __PROJECT_NAME__.Bots;

namespace __PROJECT_NAME__
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
            #if (!UseCore31)
            services.AddControllers().AddNewtonsoftJson();
            #else
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            #endif

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, EchoBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        #if (!UseCore31)
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            //app.UseHttpsRedirection();
        }
        #else

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            //app.UseHttpsRedirection();
            app.UseMvc();
        }
        #endif
    }
}
