using System.Text.Json;
using Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Models.Users;

namespace Api;

public class MessageFunctions(
    ILogger<MessageFunctions> logger, 
    WebPubSub webPubSub)
{
    private readonly ILogger<MessageFunctions> _logger = logger;

    [Function(nameof(MessageFunctions) + "_SendToAll")]
    public async Task<IActionResult> SendToAll(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "post",
            Route = "message/all")] 
        HttpRequest req)
    {
        var message = await req.ReadFromJsonAsync<MessageAll>();

        if (message is null)
        {
            return new BadRequestObjectResult("Invalid message");
        }
        
        var messageTexts = 
            HeroMessages.Messages
                .Where(m => m.FromHeroId.ToString() == message.FromPersonId)
                .ToList();
        var messageText = messageTexts.Count == 0 
            ? RandomMessageGenerator() 
            : messageTexts.ElementAt(new Random().Next(messageTexts.Count)).Message;

        var webPubSubServiceClient = webPubSub.Client;
        var sentMessage = new Message(messageText, message.FromPersonId, DateTime.UtcNow);
        var sentMessageJson = JsonSerializer.Serialize(sentMessage, JsonSerializerOptions.Web);
        await webPubSubServiceClient.SendToAllAsync(sentMessageJson);
        
        return new NoContentResult();
    }
    
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
    
    private static string RandomMessageGenerator()
    {
        // Create a random message from fake static, to I got nothing to say to you, to something just bizar
        var fallbackMessages = new List<string>
        {
            "I got nothing to say to you.",
            "zzzzzzzz <static> xcxcxcxcxc <crackle> zzzzzzz",
            "If you stir pancake batter too long, the bananas get offended.",
            "Sorry, I’m busy rearranging my sock drawer alphabetically.",
            "Once I tried to bake a cake in the mailbox, results were inconclusive.",
            "Did you know rubber ducks can't whistle in space?",
            "<silence> ... ... ... <static>"
        };
        var randomIndex = new Random().Next(fallbackMessages.Count);
        var randomMessage = fallbackMessages[randomIndex];
        return randomMessage;
    }
}