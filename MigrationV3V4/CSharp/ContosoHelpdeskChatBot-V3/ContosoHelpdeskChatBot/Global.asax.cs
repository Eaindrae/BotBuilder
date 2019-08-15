using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Azure;
using System.Web.Http;
using Autofac.Integration.WebApi;
using System.Reflection;
using ContosoHelpdeskChatBot.Dialogs;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Builder.Skills.V3;

namespace ContosoHelpdeskChatBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            BotConfig.UpdateConversationContainer();

            log4net.Config.XmlConfigurator.Configure();
        }

        //setting Bot data store policy to use last write win
        //example if bot service got restarted, existing conversation would just overwrite data to store
        public static class BotConfig
        {
            public static void UpdateConversationContainer()
            {
                var store = new InMemoryDataStore();

                Conversation.UpdateContainer(
                           builder =>
                           {
                               builder.RegisterModule(new ReflectionSurrogateModule());
                               builder.RegisterModule<SkillWebSocketModule>();

                               builder.Register(c => store)
                                         .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                                         .AsSelf()
                                         .SingleInstance();

                               builder.Register(c => new CachingBotDataStore(store,
                                          CachingBotDataStoreConsistencyPolicy
                                          .ETagBasedConsistency))
                                          .As<IBotDataStore<BotData>>()
                                          .AsSelf()
                                          .InstancePerLifetimeScope();

                               builder.Register(c => new CancelScorable(c.Resolve<IDialogTask>()))
                                    .As<IScorable<IActivity, double>>()
                                    .InstancePerLifetimeScope();

                               builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                           });
                GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(Conversation.Container);
            }
        }
    }
}
