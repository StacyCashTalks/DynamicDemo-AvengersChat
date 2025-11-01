using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using Models.Models;
using Websocket.Client;

namespace Client.Services;

public class WebPubSubService(HttpClient httpClient) : IDisposable
{
    private WebsocketClient? _webSocket;

    public event Action<Message>? OnMessageReceived;
    public event Action<bool>? OnConnectionStateChanged;

    public bool IsConnected => _webSocket?.NativeClient?.State == WebSocketState.Open;
    public bool IsConnecting { get; private set; }
    public string? CurrentUserId { get; private set; }

    public async Task<bool> ConnectAsync(string? userId = null)
    {
        if (IsConnected)
        {
            return true;
        }
        
        IsConnecting = true;
        
        try
        {
            var connectionInfo = await httpClient.GetFromJsonAsync<ConnectionResponse>(
                $"api/GetWebPubSubConnection/{userId}");
            if (connectionInfo is null)
            {
                Console.WriteLine("Unable to retrieve connection string");
                return false;
            }
            CurrentUserId = connectionInfo.UserId;

            _webSocket = new WebsocketClient(connectionInfo.Uri!);
            _webSocket.ReconnectTimeout = TimeSpan.FromHours(10);
            _webSocket.MessageReceived.Subscribe(
                msg => DeserializeMessage(msg));
            await _webSocket.Start();
            OnConnectionStateChanged?.Invoke(true);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
            return false;
        }
        finally
        {
            IsConnecting = false;
        }
    }

    private void DeserializeMessage(ResponseMessage msg)
    {
        var messageJson = msg.Text;
        if (string.IsNullOrWhiteSpace(messageJson))
        {
            return;
        }
        var message = JsonSerializer.Deserialize<Message>(messageJson, JsonSerializerOptions.Web);
        if (message is null)
        {
            return;
        }
        
        OnMessageReceived?.Invoke(message);
    }

    public async Task DisconnectAsync()
    {
        if (_webSocket is { NativeClient.State: WebSocketState.Open })
        {
            await _webSocket.Stop(WebSocketCloseStatus.NormalClosure, "Closing");
        }

        CurrentUserId = null;
            
        _webSocket?.Dispose();
        _webSocket = null;
        
        OnConnectionStateChanged?.Invoke(false);
    }

    public void Dispose()
    {
        _webSocket?.Dispose();
        httpClient.Dispose();
    }

}