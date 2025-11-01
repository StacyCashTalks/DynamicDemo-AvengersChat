namespace Models.Models;

public record ConnectionResponse(
    Uri? Uri,
    string UserId,
    string HubName);