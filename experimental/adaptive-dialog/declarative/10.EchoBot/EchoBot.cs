// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Steps;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using static Microsoft.Bot.Builder.Dialogs.Debugging.Source;

namespace Microsoft.BotBuilderSamples
{
    public class EchoBot : ActivityHandler
    {
        private IStatePropertyAccessor<DialogState> dialogStateAccessor;
        private IStatePropertyAccessor<string> environmentStateAccessor;
        private ConcurrentDictionary<string, AdaptiveDialog> rootDialogs;
        private readonly ConcurrentDictionary<string, ResourceExplorer> resourceExplorers;
        private readonly IConfiguration config;

        public EchoBot(ConversationState conversationState, ResourceExplorer resourceExplorer, IConfiguration config, IHostingEnvironment hostingEnvironment)
        {
            this.config = config;
            this.dialogStateAccessor = conversationState.CreateProperty<DialogState>("RootDialogState");
            this.environmentStateAccessor = conversationState.CreateProperty<string>("environment");
            this.resourceExplorers = new ConcurrentDictionary<string, ResourceExplorer>();

            var artifactRoot = Path.Combine(hostingEnvironment.ContentRootPath, config.GetSection("bot")["artifacts"]);
            // auto reload dialogs when file changes
            resourceExplorers = new ConcurrentDictionary<string, ResourceExplorer>(
                Environments.ToDictionary(env => env,
                env =>
                {
                    var explorer = new ResourceExplorer(resourceExplorer,
                        rp =>
                        {
                            if (rp is FolderResourceProvider)
                            {
                                var rootPath = rp.Id;
                                var relativeTo = Path.GetRelativePath(artifactRoot, rootPath);

                                if (relativeTo == ".")
                                {
                                    return new FolderResourceProvider(Path.Combine(artifactRoot, env));
                                }
                            }

                            return rp;
                        });

                    explorer.Changed += (resources) =>
                    {
                        var paths = resources.Select(x => x.Id);
                        if (paths.Any(p => Path.GetExtension(p) == ".dialog"))
                        {
                            Task.Run(() =>
                            {
                                rootDialogs[env] = LoadDialogs(explorer);
                            });
                        }
                    };

                    return explorer;
                }));


            rootDialogs = new ConcurrentDictionary<string, AdaptiveDialog>(
                Environments.ToDictionary(env => env, env => LoadDialogs(resourceExplorers[env])));
        }


        private IEnumerable<string> Environments { get
            {
                return this.config.GetSection("bot")
                    ?.GetSection("environments")
                    ?.Get<string[]>()
                    ?? new[] { "production" };
            }
        }

        private AdaptiveDialog LoadDialogs(ResourceExplorer explorer)
        {
            System.Diagnostics.Trace.TraceInformation("Loading resources...");
            var root = config.GetSection("bot").GetValue<string>("root");
            System.Diagnostics.Trace.TraceInformation($"Loading root dialog {root}");
            var resource = explorer.GetResource(root);
            var dialog = DeclarativeTypeLoader.Load<AdaptiveDialog>(resource, explorer, DebugSupport.SourceRegistry);

            System.Diagnostics.Trace.TraceInformation("Done loading resources from");

            return dialog;
        }

        protected async override Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // check if the context specified an environment
            var env = await this.environmentStateAccessor.GetAsync(turnContext, () => "production").ConfigureAwait(false);
            var rootDialog = rootDialogs[env];
            await new DialogManager(rootDialog).OnTurnAsync(turnContext, null, cancellationToken).ConfigureAwait(false);
        }

        protected override Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return base.OnMembersAddedAsync(membersAdded, turnContext, cancellationToken);
        }
    }
}
