using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;

namespace EnterpriseChannelHelloBot
{
    public class EchoBot : IBot
    {
        private const string ConnectionName = "GitConnection";
        private const string GraphApiResourceUrl = "https://graph.microsoft.com/";
        private const string ArmResourceUrl = "https://management.core.windows.net/";
        private const string SignInDialog = "SignIn";

        EchoBotAccessors _accessors;
        private readonly OAuthPrompt _signInPrompt;
        private DialogSet _dialogs;

        public EchoBot(EchoBotAccessors accessors)
        {
            _accessors = accessors;

            _dialogs = new DialogSet(accessors.ConversationDialogState);

            _signInPrompt = new OAuthPrompt(SignInDialog, new OAuthPromptSettings() { ConnectionName = ConnectionName, Text = "Sign In", Title = "Please Sign In" });
            
            _dialogs.Add(_signInPrompt);
        }

        /// <summary>
        /// Every Conversation turn for our EchoBot will call this method. In here
        /// the bot checks the Activty type to verify it's a message, bumps the 
        /// turn conversation 'Turn' count, and then echoes the users typing
        /// back to them. 
        /// </summary>
        /// <param name="turnContext">Turn scoped context containing all the data needed
        /// for processing this conversation turn. </param>        
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            DialogTurnResult result;
            DialogContext dc;

            var state = await _accessors.EchoBotConversationState.GetAsync(turnContext, () => new EchoBotConversationState());
            
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    var text = turnContext.Activity.Text.ToLowerInvariant();
                    if (text == "reset")
                    {
                        state.IsActiveSignin = false;
                    }
                    else
                    {
                        if (state.IsActiveSignin)
                        {
                            await turnContext.SendActivityAsync($"Bot received a {turnContext.Activity.Type} Activity.");
                            dc = await this._dialogs.CreateContextAsync(turnContext, cancellationToken);
                            result = await dc.ContinueDialogAsync(cancellationToken);
                            await DisplayToken(turnContext, result, state);
                        }
                        else
                        {
                            if(text.Length == 6 && int.TryParse(text, out int code))
                            {
                                var token = await ((BotFrameworkAdapter)turnContext.Adapter).GetUserTokenAsync(turnContext, ConnectionName, text, CancellationToken.None);
                                await turnContext.SendActivityAsync($"Token:");

                                if (token == null)
                                {
                                    await turnContext.SendActivityAsync($"No token was returned.");
                                }
                                else
                                {
                                    await turnContext.SendActivityAsync($"{token.Token}");
                                }
                            }
                            else if (text == "link" || text == "getlink")
                            {
                                var link = await ((BotFrameworkAdapter)turnContext.Adapter).GetOauthSignInLinkAsync(turnContext, ConnectionName, CancellationToken.None);
                                await turnContext.SendActivityAsync($"Link:");
                                await turnContext.SendActivityAsync($"{link}");
                            }
                            else if (text == "signout")
                            {
                                await ((BotFrameworkAdapter)turnContext.Adapter).SignOutUserAsync(turnContext, ConnectionName);
                                await turnContext.SendActivityAsync($"You are signed out.");
                            }
                            else if (text == "gettoken" || text == "token")
                            {
                                var token = await ((BotFrameworkAdapter)turnContext.Adapter).GetUserTokenAsync(turnContext, ConnectionName, null, CancellationToken.None);
                                await turnContext.SendActivityAsync($"Token:");

                                if (token == null)
                                {
                                    await turnContext.SendActivityAsync($"No token was returned.");
                                }
                                else
                                {
                                    await turnContext.SendActivityAsync($"{token.Token}");
                                }
                            }
                            else if (text == "signin")
                            {
                                state.IsActiveSignin = true;
                                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                                result = await dialogContext.BeginDialogAsync(SignInDialog);
                                await DisplayToken(turnContext, result, state);
                            }
                            else if (text == "aad")
                            {
                                try
                                {
                                    var tokens = await ((BotFrameworkAdapter)turnContext.Adapter).GetAadTokensAsync(turnContext, ConnectionName, new string[] { GraphApiResourceUrl, ArmResourceUrl });

                                    if (tokens != null)
                                    {
                                        await turnContext.SendActivityAsync($"Received {tokens.Count} tokens from GetAadTokensAsync:");
                                        foreach (var item in tokens)
                                        {
                                            await turnContext.SendActivityAsync($"Url: '{item.Key}':");
                                            await turnContext.SendActivityAsync($"{item.Value.Token}");
                                        }
                                    }
                                    else
                                    {
                                        await turnContext.SendActivityAsync($"Received null from GetAadTokensAsync");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await turnContext.SendActivityAsync($"Got an exception calling GetAadTokensAsync: {ex.Message}");
                                }
                            }
                            else
                            {
                                // Echo back to the user whatever they typed.
                                await turnContext.SendActivityAsync($"You sent '{turnContext.Activity.Text}'.");
                            }
                        }
                    }
                    break;
                case ActivityTypes.Event:
                case ActivityTypes.Invoke:
                    await turnContext.SendActivityAsync($"Bot received a '{turnContext.Activity.Type}' Activity with name '{turnContext.Activity.Name}'");
                    dc = await this._dialogs.CreateContextAsync(turnContext, cancellationToken);
                    result = await dc.ContinueDialogAsync(cancellationToken);
                    await DisplayToken(turnContext, result, state);
                    break;
            }
        }

        private static async Task DisplayToken(ITurnContext turnContext, DialogTurnResult result, EchoBotConversationState state)
        {
            var token = result.Result as TokenResponse;
            if (token != null)
            {
                await turnContext.SendActivityAsync($"Received a token for connection '{token.ConnectionName}'");
                await turnContext.SendActivityAsync($"{token.Token}");
                state.IsActiveSignin = false;
            }
        }
    }

    public class EchoBotConversationState
    {
        public bool IsActiveSignin { get; set; }
    }
}
