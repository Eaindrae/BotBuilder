using System;
using Autofac;
using Microsoft.Bot.Builder.Autofac.Base;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.StreamingExtensions.Transport.WebSockets;

namespace Microsoft.Bot.Builder.Skills.V3
{
    public class SkillWebSocketModule : Module
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

            WebSocketServer nullSocketServer = null;
            builder.Register(context => nullSocketServer);
            builder.Register<Action<WebSocketServer>>(context => newInstance => nullSocketServer = newInstance)
                     .InstancePerLifetimeScope();
        }
    }
}
