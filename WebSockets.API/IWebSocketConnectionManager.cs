using System.Net.WebSockets;

namespace WebSockets.API;

public interface IWebSocketConnectionManager
{
    void AddWebSocketForUser(string userId, WebSocket webSocket);
    void RemoveClosedWebSocket(string userId);
    WebSocket? GetUserWebSocketConnection(string userId);
    Task SendMessageAsync(WebSocket socket, string message);
}