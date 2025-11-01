namespace Models.Models;

public record Group(string Id, string Name);
public record SuperHero(string Id, string Name, string Alias, Group Group);
