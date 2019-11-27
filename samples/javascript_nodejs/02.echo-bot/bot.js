// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityHandler, BotFrameworkHttpClient } = require('botbuilder');

class EchoBot extends ActivityHandler {
    constructor(credentials) {
        super();
        this.client = new BotFrameworkHttpClient(credentials);

        // See https://aka.ms/about-bot-activity-message to learn more about the message and other activity types.
        this.onMessage(async (context, next) => {
            // await context.sendActivity(`You said '${ context.activity.text }'`);
            const { MicrosoftAppId, SkillAppId, SkillServiceUrl, BotServiceUrl } = process.env;
            await this.client.postActivity(MicrosoftAppId, SkillAppId, SkillServiceUrl, BotServiceUrl, context.activity.conversation.id,
                context.activity);
            console.log('EchoBot.onMessage handler: Sent activity to skill');
            // By calling next() you ensure that the next BotHandler is run.
            await next();
        });
    }
}

module.exports.EchoBot = EchoBot;
