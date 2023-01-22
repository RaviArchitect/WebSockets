using System.Net.WebSockets;
using System.Text.Json;

namespace WebSockets.API;

public class NotificationMessageHandler : INotificationMessageHandler
{
    private readonly IWebSocketConnectionManager _connectionManager;
    private readonly ILogger<NotificationMessageHandler> _logger;

    public NotificationMessageHandler(ILogger<NotificationMessageHandler> logger,
        IWebSocketConnectionManager connectionManager)
    {
        _logger = logger;
        _connectionManager = connectionManager;
    }

    public async Task SendNotification(Notification notification)
    {
        var userWebSocket = _connectionManager.GetUserWebSocketConnection(notification.UserId);
        if (userWebSocket == null)
            return;
        if (userWebSocket.State == WebSocketState.Closed)
        {
            _connectionManager.RemoveClosedWebSocket(notification.UserId);
            return;
        }

        var json = JsonSerializer.Serialize(notification, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        await _connectionManager.SendMessageAsync(userWebSocket, json);
    }
}