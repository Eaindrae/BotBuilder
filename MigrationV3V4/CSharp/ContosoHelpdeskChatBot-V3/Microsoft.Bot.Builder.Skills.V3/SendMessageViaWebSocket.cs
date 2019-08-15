using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.StreamingExtensions;

namespace Microsoft.Bot.Builder.Skills.V3
{
    public sealed class SendMessageViaWebSocket : IBotToUser
    {
        private readonly IBotToUser _inner;
        ILifetimeScope _scope;

        public SendMessageViaWebSocket(IBotToUser inner, ILifetimeScope scope)
        {
            SetField.NotNull(out this._inner, nameof(inner), inner);
            SetField.NotNull(out this._scope, nameof(scope), scope);
        }

        public IMessageActivity MakeMessage()
        {
            return this._inner.MakeMessage();
        }

        public async Task PostAsync(IMessageActivity message, CancellationToken cancellationToken = default)
        {
            await Task.Factory.StartNew(() => SendViaWebSocket(message, cancellationToken)).ConfigureAwait(false);
        }

        private async Task SendViaWebSocket(IMessageActivity message, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(message?.Text))
            {
                if (string.IsNullOrWhiteSpace(message.Id))
                {
                    message.Id = Guid.NewGuid().ToString("n");
                }

                var requestPath = $"/activities/{message.Id}";
                var request = StreamingRequest.CreatePost(requestPath);
                request.SetBody(message);
                
                var server = _scope.Resolve<Microsoft.Bot.StreamingExtensions.Transport.WebSockets.WebSocketServer>();
                await server.SendAsync(request);
            }
        }
    }
}
