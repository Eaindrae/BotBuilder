using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace Microsoft.Bot.Builder.Skills.V3
{
    public class SkillWebSocketModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // Replace AlwaysSendDirect_BotToUser registration with SendMessageViaWebSocket
            builder
                   .RegisterType<SendMessageViaWebSocket>()
                   .Keyed<IBotToUser>(typeof(AlwaysSendDirect_BotToUser))
                   .InstancePerLifetimeScope();
            
            builder
                .RegisterType<WebSocketServerContainer>()
                .InstancePerLifetimeScope();

            //WebSocketServer nullSocketServer = null;
            //builder.Register(context => nullSocketServer);
            //builder.Register<Action<WebSocketServer>>(context => newInstance => nullSocketServer = newInstance)
            //         .InstancePerLifetimeScope();
        }
    }
}
