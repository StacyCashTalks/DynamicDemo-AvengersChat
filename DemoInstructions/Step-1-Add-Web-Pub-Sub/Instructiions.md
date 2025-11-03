# Step 1 - Add Web Pub Sub

In this step, you will add Azure Web Pub Sub to your Azure Static Web App to enable real-time messaging capabilities for your Avengers Chat application.

## Instructions

Of the whole project... This is where the work is done. Seriously, after this it's easy!

## Api Changes

Let's start with the API

To use WebPubSub we need to reference the SDK. Let's do that first

``` ps
azure.messaging.webpubsub
```

Now there are a couple of classes that we need to make

### WebPubSub.cs

``` csharp
using Azure.Messaging.WebPubSub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Api;

public class WebPubSub
{
    public WebPubSub(ILogger<WebPubSub> logger, IConfiguration configuration)
    {
        var connectionString = configuration["WebPubSubConnectionString"];
        var hubName = configuration["WebPubSubHubName"] ?? "notifications";
        Client = new WebPubSubServiceClient(connectionString, hubName);
    }

    public WebPubSubServiceClient Client { get; private init; }
}
```

This simply helps us connect to the Web Pub Sub service. Notice how knows nothing about the user? That's because the API doesn't care about the user, there is no connection except the HTTP requests made

And you can see here the 2 fields needed for our secrets file. And please, please always make sure that you use your dotnet secrets. It's so important for your security! If your secrets are there, you cannot accidentally check them into source control. Ask mew how I know...

### WebPubSubConnectionFunction.cs

Now we have our web pub sub, we can actually set up our connections

``` csharp
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
```

Look at that spectacular security!!!! Of courser in the real world you would want to do more validation, but for this demo this is perfect!

### MessageFunction.cs

Now that can we can send a message URL to the user, we need to actually send messages. Let's start with sending to a person

``` C#
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
```

Again, security... I know... But...

Most of this class is. just to fake messages! The random message generator is just to make sure that if there is no message defined between the heroes, we still have something to send. Because my data isn't perfect. Stupid AI data generation...

And the same with finding the message to send. Getting the MessageText. In the real world this would just be passed in. I'm not doping that here and I'll let you see why later!

But you see what is happening here. The central webpubsub simplyh takes a message and sends it to all connected users. It is that easy! 

## Program.cs

And of course, we our functions need access to the WebPubSub class, so we need to register it in the DI container

``` csharp
builder.Services.AddSingleton<WebPubSub>();
```

## Client Changes

Now to use it!

### WebPubSubService.cs

The first thing that we need to do is create a service to handle the Web Pub Sub connection This is a little lengthy, but we'll go over it

``` c#
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
```

And we need to register it with the DI, of course

``` c#
builder.Services.AddScoped<WebPubSubService>();
```

And...Add Services to our _imports

``` c#
@using Client.Services
```

That is a lot of code, but it is really just boilerplate to connect to Web Pub Sub and handle messages. The important bits are the ConnectAsync method which gets the connection URL from the API and connects to Web Pub Sub, and the DeserializeMessage method which takes incoming messages and raises an event so that the rest of the app can handle them.

We need to be able to log in

Our signin page is not going to be too spectacular, It allows you to pick a super hero from different groups, and signs in using the service we just created.

### SignIn.razor

``` c#
@page "/signin"
@inject WebPubSubService WebPubSubService
@inject NavigationManager Navigation

<PageTitle>Sign In - Select Your Hero</PageTitle>

<div class="container mt-4">
    <h1 class="mb-4">Choose Your Hero</h1>

    @foreach (var group in SuperHeros.Groups)
    {
        var heroesInGroup = SuperHeros.SuperHeroes.Where(h => h.Group.Id == group.Id).ToList();

        if (heroesInGroup.Any())
        {
            <div class="mb-5">
                <h3 class="mb-3">@group.Name</h3>
                <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
                    @foreach (var hero in heroesInGroup)
                    {
                        <div class="col">
                            <div class="card h-100 hero-card @(isSigningIn ? "disabled" : "")" @onclick="() => SignInAsHero(hero)" style="cursor: @(isSigningIn ? "not-allowed" : "pointer");">
                                <div class="card-body">
                                    <h5 class="card-title">@hero.Alias</h5>
                                    <p class="card-text text-muted">@hero.Name</p>
                                    <small class="text-secondary">@hero.Group.Name</small>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            </div>
        }
    }
</div>

<style>
    .hero-card {
        transition: transform 0.2s, box-shadow 0.2s;
    }

    .hero-card:hover {
        transform: translateY(-5px);
        box-shadow: 0 4px 8px rgba(0,0,0,0.2);
    }

    .hero-card.disabled {
        opacity: 0.6;
        pointer-events: none;
    }
</style>

@code {
    private bool isSigningIn = false;

    private async Task SignInAsHero(SuperHero hero)
    {
        if (isSigningIn) return;

        isSigningIn = true;

        if (!WebPubSubService.IsConnected)
        {
            await WebPubSubService.ConnectAsync(hero.Id);
        }

        Navigation.NavigateTo("/");
    }
}
```

### Home.razor

And our previously empty home page needs some work!

Inject the WebPubSubService at the top!

``` c#
@inject WebPubSubService WebPubSubService
```

Then we can uncomment all that code! This is just HTML for the interface, for today it's not important. The important bit is wiring up the WebPubSubService to receive messages

These bits are! When we iunitialise the component, we need to subscribe to the events from the service

``` c#
    protected override void OnInitialized()
    {
        WebPubSubService.OnMessageReceived += OnMessageReceived;
        WebPubSubService.OnConnectionStateChanged += OnConnectionStateChanged;
    }
```

And of course we need to unsubscribe

``` c#
    public void Dispose()
    {
        WebPubSubService.OnMessageReceived -= OnMessageReceived;
        WebPubSubService.OnConnectionStateChanged -= OnConnectionStateChanged;
    }
```

When the parameters change, we need to make sure that we show the right information on the screen

``` c#
    protected override void OnParametersSet()
    {
        _connectedHero = WebPubSubService.IsConnected 
            ? SuperHeros.SuperHeroes.First(sh => sh.Id == WebPubSubService.CurrentUserId) 
            : null;
    }
```

And we want to let people log out, also healthy for our WebPubSub:

``` c#
    private async Task DisconnectAsync()
    {
        await WebPubSubService.DisconnectAsync();
        NavigationManager.NavigateTo("/signin");
    }
```

And of course, when we disconnect we need to update the screen (actually, the direct to the signin page will do that for us, but still...)

``` c#
    private void OnConnectionStateChanged(bool connected)
    {
        _connectedHero = WebPubSubService.IsConnected 
            ? SuperHeros.SuperHeroes.First(sh => WebPubSubService.CurrentUserId == sh.Id) 
            : null;

        InvokeAsync(StateHasChanged);
    }
```

And lastly, when we get a message, we need to add it to our list of messages and update the screen

``` c#
    private void OnMessageReceived(Message message)
    {
        _messages.Add(message);
        InvokeAsync(StateHasChanged);
    }
```

(That removes the red squiggles, for the init and dispose methods)

That will get us connected and receiving messages! But not sending. Let's do that now...

### MessagesService.cs

``` c#
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
}
```

A super simple method to send to all connected users. There is nothing special here, it simply makes an HTTP request to our API to send the message

Add this to the program.cs as well

``` c#
builder.Services.AddScoped<MessagesService>();
```

And to call it back to the Home screen

We need to inject this service as well

``` c#
@inject MessagesService MessagesService
```

And then we can call it.

``` csharp
    private async Task SendToAllAsync()
    {
        await MessagesService.SendToAllAsync();
    }
```