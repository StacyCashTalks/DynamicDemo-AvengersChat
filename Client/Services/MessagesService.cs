using System.Net.Http.Json;
using Models.Models;

namespace Client.Services;

public class MessagesService(WebPubSubService webPubSubService, HttpClient httpClient)
{
    public async Task<bool> SendToAllAsync(string message)
    {
        if (!webPubSubService.IsConnected || string.IsNullOrWhiteSpace(webPubSubService.CurrentUserId))
        {
            return false;
        }

        try
        {
            var messageAll = new MessageAll(message, webPubSubService.CurrentUserId);
            var response = await httpClient.PostAsJsonAsync("api/message/all", messageAll);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendToGroupAsync(string groupId, string message)
    {
        if (!webPubSubService.IsConnected || string.IsNullOrWhiteSpace(webPubSubService.CurrentUserId))
        {
            return false;
        }

        try
        {
            var messageGroup = new MessageGroup(message, webPubSubService.CurrentUserId, groupId);
            var response = await httpClient.PostAsJsonAsync("api/message/group", messageGroup);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendToHeroAsync(string toPersonId, string message)
    {
        if (!webPubSubService.IsConnected || string.IsNullOrWhiteSpace(webPubSubService.CurrentUserId))
        {
            return false;
        }

        try
        {
            var messageHero = new MessageHero(message, webPubSubService.CurrentUserId, toPersonId);
            var response = await httpClient.PostAsJsonAsync("api/message/hero", messageHero);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}