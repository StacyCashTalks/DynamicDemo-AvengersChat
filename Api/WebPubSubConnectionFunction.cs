using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Models.Users;

namespace Api;

public class WebPubSubConnectionFunction(
    ILogger<WebPubSubConnectionFunction> logger, 
    WebPubSub webPubSub)
{
    [Function("GetWebPubSubConnection")]
    public async Task<IActionResult> GetConnection(
        [HttpTrigger(
            AuthorizationLevel.Anonymous, 
            "get", 
            Route = "GetWebPubSubConnection/{id}")] 
            HttpRequest req,
            string id)
    {
        try
        {
            if (SuperHeros.SuperHeroes.All(x => x.Id != id))
            {
                return new BadRequestObjectResult("Invalid ID");
            }
            
            var user = SuperHeros.SuperHeroes.First(x => x.Id == id);
            
            var webPubSubServiceClient = webPubSub.Client;

            // Generate connection URL - this is what the client will use to connect directly to Web PubSub
            var connectionUri = await webPubSubServiceClient.GetClientAccessUriAsync(
                userId: user.Id,
                roles: [],
                groups: [user.Group.Id],
                expiresAfter: TimeSpan.FromHours(1)
            );

            var response = new ConnectionResponse
            {
                Uri = connectionUri,
                UserId = id,
                HubName = webPubSubServiceClient.Hub
            };
            
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating Web PubSub connection");
            return new StatusCodeResult(500);
        }
    }
}

public class ConnectionResponse
{
    public Uri? Uri { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string HubName { get; set; } = string.Empty;
}