using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.StreamingExtensions;

namespace Microsoft.Bot.Builder.Skills.V3
{
    public sealed class SendMessageViaWebSocket : IBotToUser
    {
        IConnectorClient _client;
        private readonly IMessageActivity _toBot;
        
        public SendMessageViaWebSocket(IMessageActivity toBot, IConnectorClient client)
        {
            SetField.NotNull(out this._toBot, nameof(toBot), toBot);
            SetField.NotNull(out this._client, nameof(client), client);
        }

        public IMessageActivity MakeMessage()
        {
            var toBotActivity = (Activity)this._toBot;
            return toBotActivity.CreateReply();
        }

        public async Task PostAsync(IMessageActivity message, CancellationToken cancellationToken = default)
        {
            var serverContainer = Conversation.Container.Resolve<WebSocketServerContainer>();
            if (serverContainer.Server == null)
            {
                await this._client.Conversations.ReplyToActivityAsync((Activity)message, cancellationToken);
            }
            else
            {
                await Task.Factory.StartNew(() => SendViaWebSocket(serverContainer.Server, message, cancellationToken)).ConfigureAwait(false);
            }
        }

        private async Task SendViaWebSocket(StreamingExtensions.Transport.WebSockets.WebSocketServer server,
                                    IMessageActivity message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(message.Id))
            {
                message.Id = Guid.NewGuid().ToString("n");
            }

            var requestPath = $"/activities/{message.Id}";
            var request = StreamingRequest.CreatePost(requestPath);
            request.SetBody(message);

            await server.SendAsync(request);
        }
    }
}
