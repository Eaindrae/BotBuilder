using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.StreamingExtensions;

namespace ContosoHelpdeskChatBot.Skills
{
    //
    // Summary:
    //     This IPostToBot service converts any unhandled exceptions to a message sent to
    //     the user.
    public sealed class SendMessageViaWebSocket : IBotToUser
    {
        private readonly IBotToUser _inner;
        public static Dictionary<string, List<IMessageActivity>> Messages = new Dictionary<string, List<IMessageActivity>>();

        public SendMessageViaWebSocket(IBotToUser inner)
        {
            SetField.NotNull(out this._inner, nameof(inner), inner);
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

                await MessagesController.Server.SendAsync(request);
            }
        }
    }
}
