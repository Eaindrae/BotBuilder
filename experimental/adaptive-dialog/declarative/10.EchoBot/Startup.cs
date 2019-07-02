// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            this.Configuration = configuration;
            this.HostingEnvironment = env;

            // TODO get rid of this dependency
            TypeFactory.Configuration = Configuration;
        }

        IConfiguration Configuration { get; set; }

        IHostingEnvironment HostingEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<AzureServiceTokenProvider>();

            // Create the credential provider to be used with the Bot Framework Adapter.
            if (this.HasAppIdAndPassword)
            {
                services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            }
            else
            {
                services.AddSingleton<ICredentialProvider, MsiCredentialProvider>();
            }

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, EchoBotHttpAdapter>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            /*
            if (Configuration.GetSection("cosmos").Exists())
            {
                services.AddSingleton(new CosmosDbStorageOptions
                {
                    DatabaseId = Configuration.GetSection("cosmos")["db"],
                    CollectionId = Configuration.GetSection("cosmos")["collection"],
                    CosmosDBEndpoint = new Uri(Configuration.GetSection("cosmos")["endpoint"]),
                    AuthKey = Configuration.GetSection("cosmos")["key"]
                });
                services.AddSingleton<IStorage, CosmosDbStorage>();
            }
            else */
            {
                services.AddSingleton<IStorage, MemoryStorage>();
            }

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            var resourceExplorer = ResourceExplorer.LoadProject(HostingEnvironment.ContentRootPath);

            services.AddSingleton(resourceExplorer);

            IdentityModelEventSource.ShowPII = true;

            ChannelValidation.OpenIdMetadataUrl = Configuration.GetSection(AuthenticationConstants.BotOpenIdMetadataKey)?.Value ?? ChannelValidation.OpenIdMetadataUrl;

            // load any deployed resource
            var artifacts = this.Configuration.GetSection("bot")?["artifacts"];
            if (!string.IsNullOrEmpty(artifacts))
            {
                var artifactsPath = Path.Combine(this.HostingEnvironment.ContentRootPath, artifacts);
                if (Directory.Exists(artifactsPath))
                {
                    resourceExplorer.AddFolder(artifactsPath);
                }
                else
                {
                    if (!this.HostingEnvironment.IsDevelopment())
                    {
                        throw new ArgumentException($"{artifactsPath} doesn't exist");
                    }
                }
            }

            // Create the bot  In this case the ASP Controller is expecting an IBot.
            services.AddSingleton<IBot, EchoBot>();

            Console.WriteLine("botLoad: service registry done");
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
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            //app.UseHttpsRedirection();
            app.UseMvc();
        }

        private bool HasAppIdAndPassword =>
            !string.IsNullOrEmpty(Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value) &&
            !string.IsNullOrEmpty(Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value);
    }
}
