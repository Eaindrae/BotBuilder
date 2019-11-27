const dotenv = require('dotenv');
const path = require('path');
const restify = require('restify');

const { BotFrameworkAdapter } = require('botbuilder');

// This bot's main dialog.
const { SkillBot } = require('./skillBot');

const ENV_FILE = path.join(__dirname, '.env');
dotenv.config({ path: ENV_FILE });

const server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 4978, () => {
    console.log('Skill listening on Port 4978.');
});

const adapter = new BotFrameworkAdapter({
    appId: process.env.SkillAppId,
    appPassword: process.env.SkillAppPassword
});

adapter.onTurnError = async (context, error) => {
    // This check writes out errors to console log .vs. app insights.
    // NOTE: In production environment, you should consider logging this to Azure
    //       application insights.
    console.error(`\n [onTurnError] unhandled error: ${ error }`);

    // Send a trace activity, which will be displayed in Bot Framework Emulator
    await context.sendTraceActivity(
        'OnTurnError Trace',
        `${ error }`,
        'https://www.botframework.com/schemas/error',
        'TurnError'
    );

    // Send a message to the user
    await context.sendActivity('The bot encounted an error or bug.');
    await context.sendActivity('To continue to run this bot, please fix the bot source code.');
};

const myBot = new SkillBot();

server.post('/api/skill', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        // Route to main dialog.
        await myBot.run(context);
    });
});
