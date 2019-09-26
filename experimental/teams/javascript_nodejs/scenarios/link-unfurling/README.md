# LinkUnfurling

This bot has been created using [Bot Framework](https://dev.botframework.com), it demonstrate link unfurling feature in Teams channel.

## Prerequisites

This sample **requires** prerequisites in order to run.

### Clone the repository

```bash
git clone https://github.com/Microsoft/botbuilder-samples.git
```

### Ngrok setup

1. Download and install [Ngrok](https://ngrok.com/download)
2. In terminal navigate to the directory where Ngrok is installed
3. Run this command: ```ngrok http -host-header=rewrite 3978 ```
4. Copy the https://xxxxxxxx.ngrok.io address and put it into notepad. 
  >**NOTE** : You want the `https` address.

### Azure Set up to provision bot with Team Channel enabled

1. Login to the [Azure Portal](https://portal.azure.com) 
2. (optional) create a new resource group if you don't currently have one
3. Go to your resource group 
4. Click "Create a new resource" 
5. Search for "Bot Channel Registration" 
6. Click Create 
7. Enter bot name, subscription
8. In the "Messaging endpoint url" enter the ngrok address from earlier. 
9. Finish the url with "/api/messages. It should look like ```https://xxxxxxxxx.ngrok.io/api/messages```
10. Click the "Microsoft App Id and password" box 
11. Click on "Create New" 
12. Click on "Create App ID in the App Registration Portal" 
13. Click "New registration" 
14. Enter a name 
15. Under "Supported account types" select "Accounts in any organizational directory and personal Microsoft accounts" 
16. Click register 
17. Copy the application (client) ID and put it in Notepad. Label it "Microsoft App ID" 
18. Go to "Certificates & Secrets" 
19. Click "+ New client secret" 
20. Enter a description 
21. Click "Add" 
22. Copy the value and put it into Notepad. Label it "Password"
23. (back in the channel registration view) Copy/Paste the Microsoft App ID and Password into their respective fields 
24. Click Create 
25. Go to "Resource groups" on the left 
26. Select the resource group that the bot channel reg was created in 
27. Select the bot channel registration 
28. Go to Settings  
29. Select the "Teams" icon under "Add a featured channel 
30. Click Save 



### Updating Sample Settings

1. Open `.env` file.
2. Enter the app id under the `MicrosoftAppId` and the password under the `MicrosoftAppPassword`. 
3. Save the close the file.
4. Under the `TeamsAppManifest` folder open the `manifest.json` file.
5. Update the ```botId``` fields with the Microsoft App ID from before  (2 places)
6. Update the ```id``` with the Microsoft App ID from before 



### Uploading the bot to Teams

1. In file explorer navigate to the `teams-app-manifest` folder in the project 
2. Select the 3 files (`color.png`, `manifest.json` and `outline.png`) and zip them. 
3. Open Teams 
4. Click on "Apps" 
5. Select "Upload a custom app" on the left at the bottom 
6. Select the zip  
7. Select for you  
8. (optionally) click install if prompted 
9. Click open 

   

## To try this sample

- In a terminal, navigate to bot folder (ie `link-unfurling`).

It's suggested you run the bot twice.  First with recording turned on, second with recording turned off.

```bash
# Install
npm install

# Set record mode - true or false
  # Windows
  set AZURE_NOCK_RECORD=true
  # Powershell
  $env:AZURE_NOCK_RECORD="true"

# run the bot
npm start
```

### Interacting with the bot

1. Send a message to your bot in Teams.
2. Type any valid https url, ie https://www.bing.com, wait until the url is bolded, hit the space key and you should see a thumbnail card for the url info populated, like below. 
   ![Sample Unfurl](./_images/1569017810114.png)
3. If recording, as you send messages, you should see new text files in the `./recordings` directory.

### Notes
1. Url unfurls are cached.  Try using different https `.com` sites each time.
2. If you install multiple bots which handle link unfurling, the first bot that responds will be displayed.
3. If the bot returns multiple results, the first result will be displayed.
4. Link unfurling action is handled by `onAppBasedLinkQuery` method in the bot code
