using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Skills.V3
{
    public interface IProcessActivity
    {
        Task<HttpResponseMessage> ProcessActivityAsync(Activity activity);
    }
}
