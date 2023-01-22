using System.Text.Json;

namespace WebSockets.API;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedTimeInUtc { get; set; }
    public int TypeId { get; set; }
    public string TypeName { get; set; } = null!;
    public bool IsTapped { get; set; }
    public JsonElement Payload { get; set; }
}