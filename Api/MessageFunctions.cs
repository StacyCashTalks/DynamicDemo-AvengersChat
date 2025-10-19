using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;

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

        var correctedMessage = "zzzz <static> zzzzzz <crackle>";
        if (!string.IsNullOrWhiteSpace(message.Message))
        {
            correctedMessage = message.Message;       
        }
        
        var webPubSubServiceClient = webPubSub.Client;
        
        var sentMessage = new Message(correctedMessage, message.FromPersonId, DateTime.UtcNow);
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
        
        var correctedMessage = "zzzz <static> zzzzzz <crackle>";
        if (!string.IsNullOrWhiteSpace(message.Message))
        {
            correctedMessage = message.Message;       
        }
        
        var webPubSubServiceClient = webPubSub.Client;
        
        var sentMessage = new Message(correctedMessage, message.FromPersonId, DateTime.UtcNow);
        var sentMessageJson = JsonSerializer.Serialize(sentMessage, JsonSerializerOptions.Web);
        await webPubSubServiceClient.SendToGroupAsync(message.ToGroupId, sentMessageJson);
        
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
        
        var correctedMessage = "zzzz <static> zzzzzz <crackle>";
        if (!string.IsNullOrWhiteSpace(message.Message))
        {
            correctedMessage = message.Message;       
        }
        
        var webPubSubServiceClient = webPubSub.Client;
        
        var sentMessage = new Message(correctedMessage, message.FromPersonId, DateTime.UtcNow);
        var sentMessageJson = JsonSerializer.Serialize(sentMessage, JsonSerializerOptions.Web);
        await webPubSubServiceClient.SendToUserAsync(message.ToPersonId, sentMessageJson);
        
        return new NoContentResult();
    }    
}