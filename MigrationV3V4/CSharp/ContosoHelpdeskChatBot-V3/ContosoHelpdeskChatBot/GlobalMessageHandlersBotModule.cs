namespace ContosoHelpdeskChatBot
{
    using Autofac;
    using ContosoHelpdeskChatBot.Skills;
    using Dialogs;
    using Microsoft.Bot.Builder.Autofac.Base;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;

    public class GlobalMessageHandlersBotModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // Intecept all sent messages so we can send out via WebSockets (SendMessageViaWebSocket)

            builder.RegisterKeyedType<SendMessageViaWebSocket, IBotToUser>().InstancePerLifetimeScope();
            builder
                .RegisterAdapterChain<IBotToUser>
                (
                    typeof(AlwaysSendDirect_BotToUser),
                    typeof(MapToChannelData_BotToUser),
                    typeof(LogBotToUser),
                    typeof(SendMessageViaWebSocket)
                )
                .InstancePerLifetimeScope();

            builder
                .Register(c => new CancelScorable(c.Resolve<IDialogTask>()))
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
        }
    }
}
