using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;

namespace AuthTest
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //// Configure credentials
            //services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            //// Register the skills configuration class
            //services.AddSingleton<SkillsConfiguration>();

            //// Register AuthConfiguration to enable custom claim validation.
            //services.AddSingleton(sp => new AuthenticationConfiguration { ClaimsValidator = new AllowedSkillsClaimsValidator(sp.GetService<SkillsConfiguration>()) });

            //// Register the skills client and skills request handler.
            //services.AddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
            //services.AddSingleton<ITokenExchangeConfig>(new TokenExchangeConfig() { ProviderId = null, ConnectionName = Configuration.GetSection("ConnectionName")?.Value });
            //services.AddHttpClient<SkillHttpClient>();
            //services.AddSingleton<ChannelServiceHandler, TokenExchangeSkillHandler>();

            //// Create the Bot Framework Adapter with error handling enabled.
            //services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            //services.AddSingleton<BotAdapter>(sp => (BotAdapter)sp.GetService<IBotFrameworkHttpAdapter>());

            //// Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            //services.AddSingleton<IStorage, MemoryStorage>();

            //// Create the User state. (Used in this bot's Dialog implementation.)
            //services.AddSingleton<UserState>();

            //// Create the Conversation state. (Used by the Dialog system itself.)
            //services.AddSingleton<ConversationState>();

            //// The Dialog that will be run by the bot.
            //services.AddSingleton<MainDialog>();

            //// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            //services.AddTransient<IBot, AuthBot<MainDialog>>();

            ////To make it work on scratch
            //OAuthClientConfig.OAuthEndpoint = "https://api.ppe.botframework.com";
            //MicrosoftAppCredentials.TrustServiceUrl("https://api.ppe.botframework.com");

            services.AddDefaultIdentity<IdentityUser>().AddDefaultUI();
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseMvc();
        }
    }
}
