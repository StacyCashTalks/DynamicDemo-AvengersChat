namespace Api.ApiModels;

public record LibraryUser(string Id, string Provider, string UserDetails, string Type = "User");