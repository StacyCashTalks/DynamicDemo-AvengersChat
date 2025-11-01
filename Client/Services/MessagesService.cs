using System.Net.Http.Json;
using Models.Models;

namespace Client.Services;

public class MessagesService(WebPubSubService webPubSubService, HttpClient httpClient)
{
    public async Task<bool> SendToAllAsync()
    {
        if (!webPubSubService.IsConnected || string.IsNullOrWhiteSpace(webPubSubService.CurrentUserId))
        {
            return false;
        }

        try
        {
            var messageAll = new MessageAll(webPubSubService.CurrentUserId);
            var response = await httpClient.PostAsJsonAsync("api/message/all", messageAll);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

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
}