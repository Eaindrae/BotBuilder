using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Protocol;
using Microsoft.Bot.Protocol.WebSockets;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ContosoHelpdeskChatBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public static Microsoft.Bot.StreamingExtensions.Transport.WebSockets.WebSocketServer Server;

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }


        public HttpResponseMessage Get()
        {
            var currentContext = HttpContext.Current;
            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {
                currentContext.AcceptWebSocketRequest(ProcessWebsocketSession) ;
            }

            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }

        private async Task ProcessWebsocketSession(AspNetWebSocketContext context)
        {
            // TODO Validate Authorization Header
            //if (context.Headers["Authorization"] != null)
            //{}

            WebSocket socket = context.WebSocket;

            var handler = new SkillWebSocketRequestHandler();
            Server = new Microsoft.Bot.StreamingExtensions.Transport.WebSockets.WebSocketServer(socket, handler);
            var startListening = Server.StartAsync();

            Task.WaitAll(startListening);
        }
    }

    internal class SkillWebSocketRequestHandler : Microsoft.Bot.StreamingExtensions.RequestHandler
    {
        internal SkillWebSocketRequestHandler()
        {
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

            var cancellationTokenSource = new CancellationTokenSource();

            // Invoke Bot
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }

            response.StatusCode = (int)HttpStatusCode.OK;

            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}
