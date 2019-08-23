using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Skills.V3
{
    public abstract class SkillMessagesControllerBase : ApiController, IProcessActivity
    {
        public HttpResponseMessage Get()
        {
            var currentContext = HttpContext.Current;
            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {
                currentContext.AcceptWebSocketRequest(ProcessWebsocketSession);
            }

            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }

        public abstract Task<HttpResponseMessage> ProcessActivityAsync(Activity activity);

        private async Task ProcessWebsocketSession(AspNetWebSocketContext context)
        {
            // TODO Validate Authorization Header
            //if (context.Headers["Authorization"] != null)
            //{}

            using (var scope = Conversation.Container.BeginLifetimeScope())
            {
                WebSocket socket = context.WebSocket;

                var handler = new SkillWebSocketRequestHandler(this);
                var server = new Microsoft.Bot.StreamingExtensions.Transport.WebSockets.WebSocketServer(socket, handler);
                
                // Update the current WebSocketServer in the container, so SendMessageViaWebSocket.SendViaWebSocket is
                // able to resolve the correct socket server
                var serverContainer = scope.Resolve<WebSocketServerContainer>();
                serverContainer.Server = server;
                
                var startListening = server.StartAsync();
                Task.WaitAll(startListening);
            }
        }
    }
}
