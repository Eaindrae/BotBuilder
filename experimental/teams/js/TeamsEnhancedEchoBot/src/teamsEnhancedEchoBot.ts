// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import {
    Activity,
    MessageFactory,
    TeamsActivityHandler,
    TeamsInfo,
    TurnContext,
    ActivityTypes,
    Mention
} from 'botbuilder';

//
// You need to install this bot in a team. You can @mention the bot "show members", "show channels", or "show details" to get the
// members of the team, the channels of the team, or metadata about the team respectively.
//
export class TeamsEnhancedEchoBot extends TeamsActivityHandler {
    protected dict: {};

    constructor(dict: {}) {
        super();

        this.dict = dict;

        // See https://aka.ms/about-bot-activity-message to learn more about the message and other activity types.
        this.onMessage(async (context, next) => {
            await context.sendActivity(MessageFactory.text(`Echo: ${context.activity.text}`));
            TurnContext.removeRecipientMention(context.activity);
            const text = context.activity.text ? context.activity.text.trim() : '';
            switch (text) {
                case "show members":
                    await this.showMembers(context);
                    break;

                case "show channels":
                    await this.showChannels(context);
                    break;

                case "show details":
                    await this.showDetails(context);
                    break;
                
                case "delete":
                    await this.deleteMessage(context);
                    break; 
                
                case "mention":
                    await this.mentionActivity(context);
                    break;
                
                case "update":
                    await updateAllMessages(context);
                    break;

                default:
                    await context.sendActivity(
                        "Hi! I'm an more enhanced echo bot. These are the commands that I respond to:" +
                        "\"show members\", \"show channels\", \"show details\", \"delete\", \"mention\", \"update\".");
                    
                        await context.sendActivity("These are the events that I will respond to: " +
                    "Teams channel add, teams channel remove, teams channel rename, team member added, team member removed, message reaction added" +
                    "message reaction removed")
                    break;
            }
            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });

        this.onMembersAdded(async (context, next) => {
            const membersAdded = context.activity.membersAdded;
            for (const member of membersAdded) {
                if (member.id !== context.activity.recipient.id) {
                    await context.sendActivity('Hello and welcome!');
                }
            }

            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }

    private async mentionActivity(context: TurnContext): Promise<void> {
        var mention = { mentioned: context.activity.from, text:`<at>${context.activity.from.name}</at>` };

        var replyActivity = MessageFactory.text(`Hello ${mention.text}.`);
        replyActivity.entities = [ <Mention> mention ];

        await context.sendActivity(replyActivity);
    }

    private async updateAllMessages(context: TurnContext): Promise<void> {
        const text = context.activity.text;
        for (var activityId in this.dict) {
            var newActivity = MessageFactory.text(text);
            newActivity.id = activityId;
            await context.updateActivity(newActivity);
        }
    }

    private async deleteMessage(context: TurnContext): Promise<void> {
        for (var activityId in this.dict) {
            await context.deleteActivity(activityId);
        }

        this.dict = {};
    }

    private async showMembers(context: TurnContext): Promise<void> {
        let teamsChannelAccounts = await TeamsInfo.getMembers(context);
        await context.sendActivity(MessageFactory.text(`Total of ${teamsChannelAccounts.length} members are currently in team`));
        let messages = teamsChannelAccounts.map(function(teamsChannelAccount) {
            return `${teamsChannelAccount.aadObjectId} --> ${teamsChannelAccount.name} --> ${teamsChannelAccount.userPrincipalName}`;
        });
        await this.sendInBatches(context, messages);
    }
    
    private async showChannels(context: TurnContext): Promise<void> { 
        let channels = await TeamsInfo.getChannels(context);
        await context.sendActivity(MessageFactory.text(`Total of ${channels.length} channels are currently in team`));
        let messages = channels.map(function(channel) {
            return `${channel.id} --> ${channel.name ? channel.name : 'General'}`;
        });
        await this.sendInBatches(context, messages);
    }
   
    private async showDetails(context: TurnContext): Promise<void> {
        let teamDetails = await TeamsInfo.getTeamDetails(context);
        await context.sendActivity(MessageFactory.text(`The team name is ${teamDetails.name}. The team ID is ${teamDetails.id}. The AAD GroupID is ${teamDetails.aadGroupId}.`));
    }

    private async sendInBatches(context: TurnContext, messages: string[]): Promise<void> {
        let batch: string[] = [];
        messages.forEach(async (msg: string) => {
            batch.push(msg);
            if (batch.length == 10) {
                await context.sendActivity(MessageFactory.text(batch.join('<br>')));
                batch = [];
            }
        });

        if (batch.length > 0) {
            await context.sendActivity(MessageFactory.text(batch.join('<br>')));
        }
    }

    private async sendMessageAndLogActivityId(context: TurnContext, text: string): Promise<void> {
        const resourceResponse = await context.sendActivity({ text });
        this.dict[resourceResponse.id] = context.activity.text;
    }
}
