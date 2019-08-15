using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Protocol;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Skills.V3
{

    internal class SkillWebSocketRequestHandler : Microsoft.Bot.StreamingExtensions.RequestHandler
    {
        // This dictionary keeps the WebSocketServers cached, so the socket can remain open.
        public static ConcurrentDictionary<string, Microsoft.Bot.StreamingExtensions.Transport.WebSockets.WebSocketServer> Servers = new ConcurrentDictionary<string, Microsoft.Bot.StreamingExtensions.Transport.WebSockets.WebSocketServer>();

        public string Id { get; set; }
        private IProcessActivity _activityProcessor;

        internal SkillWebSocketRequestHandler(IProcessActivity activityProcessor)
        {
            _activityProcessor = activityProcessor;
            Id = Guid.NewGuid().ToString();
        }

        public async override Task<StreamingResponse> ProcessRequestAsync(Microsoft.Bot.StreamingExtensions.ReceiveRequest request, ILogger<Microsoft.Bot.StreamingExtensions.RequestHandler> logger, object context = null, CancellationToken cancellationToken = default)
        {
            var response = new StreamingResponse();

            var body = request.ReadBodyAsString();
            if (string.IsNullOrEmpty(body) || request.Streams?.Count == 0)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;

                return response;
            }

            if (request.Streams.Where(x => x.ContentType != "application/json; charset=utf-8").Any())
            {
                response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return response;
            }

            var activity = JsonConvert.DeserializeObject<Activity>(body);

            var responseMessage = await _activityProcessor.ProcessActivityAsync(activity);
            response.StatusCode = (int)responseMessage.StatusCode;
            return response;
        }
    }
}
