using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Skills.V3
{
    interface IProcessActivity
    {
        Task ProcessActivityAsync(Activity activity);
    }
}
