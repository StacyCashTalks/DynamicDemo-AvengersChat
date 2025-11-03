# Demo Avengers Chat Instructions - Step 2: Send To Person

In this step, you will modify the Avengers Chat application to allow users to send private messages to specific individuals using Azure Web Pub Sub.

## API Changes

you know, there really isn't much here... We need to receive the message and send it out...


``` csharp
    [Function(nameof(MessageFunctions) + "_SendToHero")]
    public async Task<IActionResult> SendToHero(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "post",
            Route = "message/hero")] 
        HttpRequest req)
    {
        var message = await req.ReadFromJsonAsync<MessageHero>();

        if (message is null)
        {
            return new BadRequestObjectResult("Invalid message");
        }
        
        var messageTexts = 
            HeroMessages.Messages
                .Where(m => 
                    m.FromHeroId.ToString() == message.FromPersonId
                    && m.ToHeroId.ToString() == message.ToPersonId 
                    && m.Category == "direct")
                .ToList();
        string messageText;
        messageText = messageTexts.Count == 0 
            ? RandomMessageGenerator() 
            : messageTexts.ElementAt(new Random().Next(messageTexts.Count)).Message;
        
        var webPubSubServiceClient = webPubSub.Client;
        
        var sentMessage = new Message(messageText, message.FromPersonId, DateTime.UtcNow);
        var sentMessageJson = JsonSerializer.Serialize(sentMessage, JsonSerializerOptions.Web);
        await webPubSubServiceClient.SendToUserAsync(message.ToPersonId, sentMessageJson);
        
        return new NoContentResult();
    } 
```

That is literally it... The users will get their messages sent to them via Web Pub Sub. Now we need to receive them

## Client Changes

OK, first we need to send them... Let's change the message function to send to a person

``` csharp
    public async Task<bool> SendToHeroAsync(string toPersonId)
    {
        if (!webPubSubService.IsConnected || string.IsNullOrWhiteSpace(webPubSubService.CurrentUserId))
        {
            return false;
        }

        try
        {
            var messageHero = new MessageHero(webPubSubService.CurrentUserId, toPersonId);
            var response = await httpClient.PostAsJsonAsync("api/message/hero", messageHero);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
```

And we need to wire that up to our page...

``` csharp
    private async Task SendToHeroAsync()
    {
        if (_selectedPersonId is null)
        {
            return;
        }
        await MessagesService.SendToHeroAsync(_selectedPersonId);
    }
    
```

And, er, that is it... Thew message comes in and we receive it the same way as before.

I did say that I was shocked at how easy this was once I got started, right?
