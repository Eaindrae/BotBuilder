// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const express = require('express');
const dotenv = require('dotenv');
const path = require('path');

// Import required bot services.
// See https://aka.ms/bot-services to learn more about the different parts of a bot.
const { ActivityTypes, BotFrameworkAdapter } = require('botbuilder');
const { EchoBot } = require('./bot');

const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

const adapter = new BotFrameworkAdapter({
    appId: process.env.MicrosoftAppId,
    appPassword: process.env.MicrosoftAppPassword,
    enableWebSockets: true
});

// Catch-all for errors.
adapter.onTurnError = async (context, error) => {
    // Create a trace activity that contains the error object
    const traceActivity = {
        type: ActivityTypes.Trace,
        timestamp: new Date(),
        name: 'onTurnError Trace',
        label: 'TurnError',
        value: `${ error }`,
        valueType: 'https://www.botframework.com/schemas/error'
    };
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights.
    console.error(`\n [onTurnError] unhandled error: ${ error }`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendActivity(traceActivity);

    // Send a message to the user
    await context.sendActivity(`The bot encounted an error or bug.`);
    await context.sendActivity(`To continue to run this bot, please fix the bot source code.`);
};

const bot = new EchoBot();

const app = express();

app.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        // Route to main dialog.
        await bot.run(context);
    });
});

// Express doesn't have built-in upgrade support, so we need to store the
// HTTP Server created by app.listen().
const server = app.listen(process.env.port || process.env.PORT || 3978);
console.log(`\nListening on port ${ server.address().port } at ${ server.address().address }`);

// To enable streaming on the bot, the app needs to listen for the 'upgrade' event from the HTTP server.
// Register an event handler for the 'upgrade' event: https://nodejs.org/api/http.html#http_event_upgrade_1
// Modify the request by creating a method that returns the socket and head, mirroring Restify
server.on('upgrade', (req, socket, head) => {
    // These socket.destroy() calls emit a Server error that closes the Node process.
    if (req.method !== 'GET') {
        socket.destroy(new Error(`Unexpected method received for upgrade: ${ req.method }. Only "GET" is accepted.`));
        return;
    }

    if (req.url !== '/api/messages' && req.url !== '/api/messages/') {
        socket.destroy(new Error(`Unexpected URL on upgrade request: ${ req.url }`));
        return;
    }

    // Ugly shim
    const res = {
        claimUpgrade() {
            return { socket, head };
        }
    };

    adapter.processActivity(req, res, async (context) => {
        // Route to main dialog.
        await bot.run(context);
    });
});
