using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace WebSockets.API;

public class WebSocketConnectionManager : IWebSocketConnectionManager
{
    private readonly ILogger<WebSocketConnectionManager> _logger;
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

    public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
    {
        _logger = logger;
    }

    public void AddWebSocketForUser(string userId, WebSocket webSocket)
    {
        _sockets.Remove(userId, out var previousWebSocket);
        if (previousWebSocket is not null && previousWebSocket.State == WebSocketState.Open)
            previousWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        _sockets.TryAdd(userId, webSocket);
    }

    public void RemoveClosedWebSocket(string userId)
    {
        _sockets.Remove(userId, out var userWebSocket);
    }

    public WebSocket? GetUserWebSocketConnection(string userId)
    {
        _sockets.TryGetValue(userId, out var userWebSocket);
        return userWebSocket;
    }

    public async Task SendMessageAsync(WebSocket socket, string message)
    {
        if (socket.State != WebSocketState.Open)
            return;

        await socket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(message),
                0,
                message.Length),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }
}