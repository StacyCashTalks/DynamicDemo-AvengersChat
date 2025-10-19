namespace Models.Models;

public record MessageHero(string Message, string FromPersonId, string ToPersonId);
public record MessageGroup(string Message, string FromPersonId, string ToGroupId);
public record MessageAll(string Message, string FromPersonId);
public record Message(string MessageText, string FromPersonId, DateTime SentAt);