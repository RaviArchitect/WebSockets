using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WebSockets.API.Controllers;

[Route("api/")]
[ApiController]
public class WebSocketsController : ControllerBase
{
    private readonly IWebSocketConnectionManager _connectionManager;
    private readonly ILogger<WebSocketsController> _logger;
    private readonly INotificationMessageHandler _notificationMessageHandler;


    public WebSocketsController(
        IWebSocketConnectionManager connectionManager, INotificationMessageHandler notificationMessageHandler,
        ILogger<WebSocketsController> logger)
    {
        _connectionManager = connectionManager;
        _notificationMessageHandler = notificationMessageHandler;
        _logger = logger;
    }

    [HttpGet]
    [Route("InitiateConnection")]
    public async Task InitiateWebSocketConnection(string userId)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation("WebSocket connection established for {UserId}", userId);
            _connectionManager.AddWebSocketForUser(userId, webSocket);
            await Echo(HttpContext, webSocket, userId);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }

    [HttpPost]
    [Route("TransferNotificationPayloadToWebSocket")]
    public async Task TransferNotificationPayloadToWebSocket([FromBody] Notification notification)
    {
        try
        {
            await _notificationMessageHandler.SendNotification(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while transferring notification the payload to websocket. Error: " + ex.Message);
            throw;
        }
    }

    private async Task Echo(HttpContext context, WebSocket webSocket, string userId)
    {
        try
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (receiveResult.CloseStatus.HasValue == false)
            {
                var serverMsg =
                    Encoding.UTF8.GetBytes(
                        "Server: Hello Boss, I am not a cricketer to catch your messages. Your job is to receive messages from me but not to send messages to me.");

                await webSocket.SendAsync(
                    new ArraySegment<byte>(serverMsg, 0, serverMsg.Length),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
        catch (WebSocketException ex)
        {
            _logger.LogError(ex, "WebSocket exception occurred. Error: " + ex.Message);
            if (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                _connectionManager.RemoveClosedWebSocket(userId);
        }
    }
}