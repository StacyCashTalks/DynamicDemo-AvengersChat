namespace AvengersComms.Models;

public record MessageHero(string FromPersonId, string ToPersonId);
public record MessageGroup(string FromPersonId);
public record MessageAll(string FromPersonId);
public record Message(string MessageText, string FromPersonId, DateTime SentAt);