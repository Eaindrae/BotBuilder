// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { config } from 'dotenv';
import * as path from 'path';
import * as restify from 'restify';


import * as fs from 'fs';


// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
import { BotFrameworkAdapter, WebResponse } from 'botbuilder';

// This bot's main dialog.
import { LinkUnfurlingBot } from './teamsLinkUnfurling';


// Set up Nock
import * as nockHelper from './../src/nock-helper';
nockHelper.nockHttp('link-unfurling')


const ENV_FILE = path.join(__dirname, '..', '.env');
config({ path: ENV_FILE });

// Create adapter.
// See https://aka.ms/about-bot-adapter to learn more about adapters.

const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword,
});

// Catch-all for errors.
adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    console.error('[onTurnError]:');
    console.error(error);
    // Send a message to the user
    await context.sendActivity(`Oops. Something went wrong in the bot!\n  ${error.message}`);
};

// Create the main dialog.
const myBot = new LinkUnfurlingBot();

if (nockHelper.isRecording()) {
    // Create HTTP server.
    const server = restify.createServer();
    server.listen(process.env.port || process.env.PORT || 3978, () => {
        console.log(`\n${server.name} listening to ${server.url}`);
        console.log(`\nGet Bot Framework Emulator: https://aka.ms/botframework-emulator`);
        console.log(`\nTo test your bot, see: https://aka.ms/debug-with-emulator`);
    });
    
    // Listen for incoming requests.
    server.post({
        path: '/api/messages',
        contentType: 'application/json'
    },
    //server.post('/api/messages', 
    (req, res, next) => {
        adapter.processActivity(req, res, async (context) => {
            if (req.body.text == 'exit') {
                //graceful shutdown
                process.exit();
            }
            nockHelper.logRequest(req, 'link-unfurling');
            // Route to main dialog.
            await myBot.run(context);
        });
    });
}
else if (nockHelper.isPlaying()) {
    nockHelper.processRecordings('link-unfurling', adapter, myBot);
}
else if (nockHelper.isProxyHost()) {
    // Create HTTP proxy server.
    nockHelper.proxyRecordings();
}
else if (nockHelper.isProxyPlay()) {
    nockHelper.proxyPlay(myBot);
}
