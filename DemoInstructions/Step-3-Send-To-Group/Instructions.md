# Demo Avengers Chat Instructions - Step 3: Send To Group

## API Changes

As before there are  not many needed here. There are only two changes to make to the `chat` API.

MessaageFunctions.cs

``` csharp
[Function(nameof(MessageFunctions) + "_SendToGroup")]
    public async Task<IActionResult> SendToGroup(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "post",
            Route = "message/group")] 
        HttpRequest req)
    {
        var message = await req.ReadFromJsonAsync<MessageGroup>();

        if (message is null)
        {
            return new BadRequestObjectResult("Invalid message");
        }
        
        var hero = SuperHeros.SuperHeroes.FirstOrDefault(x => x.Id == message.FromPersonId);

        if (hero is null)
        {
            return new BadRequestObjectResult("Invalid message");       
        }
        
        var messageTexts = 
            HeroMessages.Messages
                .Where(m => 
                    m.FromHeroId.ToString() == message.FromPersonId
                    && m.ToHeroId.ToString() == hero.Group.Id 
                    && m.Category == "group")
                .ToList();
        var messageText = messageTexts.Count == 0 
            ? RandomMessageGenerator() 
            : messageTexts.ElementAt(new Random().Next(messageTexts.Count)).Message;
        
        var webPubSubServiceClient = webPubSub.Client;
        
        var sentMessage = new Message(messageText, message.FromPersonId, DateTime.UtcNow);
        var sentMessageJson = JsonSerializer.Serialize(sentMessage, JsonSerializerOptions.Web);
        await webPubSubServiceClient.SendToGroupAsync(hero.Group.Id, sentMessageJson);
        
        return new NoContentResult();
    }
```

That is how we can we can send messages, but what about receiving them?

For that we need to change the WebPubSubConnectionFunction.cs

``` csharp
                groups: [user.Group.Id],
```

That is really it for the API! We could go further and add Roles (Team leads, techies, etc) but for this demo we will keep it simple.

## Client Changes

``` C#

    public async Task<bool> SendToGroupAsync()
    {
        if (!webPubSubService.IsConnected || string.IsNullOrWhiteSpace(webPubSubService.CurrentUserId))
        {
            return false;
        }

        try
        {
            var messageGroup = new MessageGroup(webPubSubService.CurrentUserId);
            var response = await httpClient.PostAsJsonAsync("api/message/group", messageGroup);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
 ```

Replace the empty SendToGroupAsync method with the above below

``` csharp
    private async Task SendToGroupAsync()
    {
        if (_connectedHero is null)
        {
            return;
        }
        
        await MessagesService.SendToGroupAsync();
    }
```

That's it. Lets run it local, and then we can see it live!
