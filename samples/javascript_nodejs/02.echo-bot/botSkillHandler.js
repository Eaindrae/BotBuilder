const { ChannelServiceHandler } = require('botbuilder');

class BotSkillHandler extends ChannelServiceHandler {
    constructor(adapter, credentialProvider, authConfig) {
        super(credentialProvider, authConfig);
        this.adapter = adapter;
    }

    async handleSendToConversation(authHeader, convId, activity) {
        console.log(' - BotSkillHandler.handleSendToConversation() has been called.');
        return super.handleSendToConversation(authHeader, convId, activity);
    }

    async onSendToConversation(claimsIdentity, convId, activity) {
        const connectorClient = this.adapter.createConnectorClient(activity.serviceUrl);
        await connectorClient.conversations.sendToConversation(convId, activity);
    }
}

module.exports.BotSkillHandler = BotSkillHandler;
