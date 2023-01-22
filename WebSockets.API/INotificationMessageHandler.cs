namespace WebSockets.API;

public interface INotificationMessageHandler
{
    Task SendNotification(Notification notification);
}