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
                currentContext.AcceptWebSocketRequest(ProcessWebsocketSession) ;
            }

            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }

        public abstract Task<HttpResponseMessage> ProcessActivityAsync(Activity activity);

        private async Task ProcessWebsocketSession(AspNetWebSocketContext context)
        {
            // TODO Validate Authorization Header
            //if (context.Headers["Authorization"] != null)
            //{}
            WebSocket socket = context.WebSocket;

            var handler = new SkillWebSocketRequestHandler(this);
            var server = new Microsoft.Bot.StreamingExtensions.Transport.WebSockets.WebSocketServer(socket, handler);
            SkillWebSocketRequestHandler.Servers.TryAdd(server.GetHashCode().ToString(), server);

            // Update the current WebSocketServer in the container, so SendMessageViaWebSocket.SendViaWebSocket is
            // able to resolve the correct socket server
            var serverContainer = Conversation.Container.Resolve<WebSocketServerContainer>();
            serverContainer.Server = server;
            server.Disconnected += Server_Disconnected;

            var startListening = server.StartAsync();
            Task.WaitAll(startListening);
        }

        private void Server_Disconnected(object sender, Microsoft.Bot.StreamingExtensions.Transport.DisconnectedEventArgs e)
        {
            var asServer = sender as Microsoft.Bot.StreamingExtensions.Transport.WebSockets.WebSocketServer;

            SkillWebSocketRequestHandler.Servers.TryRemove(asServer.GetHashCode().ToString(), out asServer);
        }
    }
}
