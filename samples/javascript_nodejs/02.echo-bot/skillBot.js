const { ActivityHandler } = require('botbuilder');

class SkillBot extends ActivityHandler {
    async run(context) {
        console.log('SkillBot.run() called!');
        await context.sendActivity('You reached the skill');
    }
}

module.exports.SkillBot = SkillBot;
