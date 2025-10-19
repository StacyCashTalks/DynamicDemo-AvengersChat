using Models.Models;

namespace Models.Users;

public static class SuperHeros
{
    public static Group[] Groups { get; set; } =
    [
        new("1", "Avengers"),
        new("2", "Thunderbolts"),
        new("3", "Guardians of the Galaxy"),
        new("4", "Mystics and Magicians"),
        new("5", "Anti-Heroes")
    ];
    
    public static SuperHero[] SuperHeroes { get; set; } =
    [
        // Avengers
        new("1", "Tony Stark", "Iron Man", Groups[0]),
        new("2", "Steve Rogers", "Captain America", Groups[0]),
        new("3", "Thor Odinson", "Thor", Groups[0]),
        new("4", "Natasha Romanoff", "Black Widow", Groups[0]),
        new("5", "Bruce Banner", "The Hulk", Groups[0]),
    
        // Thunderbolts
        new("6", "Ava Starr", "Ghost", Groups[1]),
        new("7", "James Rupert", "Bucky Barnes", Groups[1]),
        new("8", "Clint Barton", "Hawkeye", Groups[1]),
        new("9", "John Walker", "U.S. Agent", Groups[1]),
    
        // Guardians of the Galaxy
        new("10", "Peter Quill", "Star-Lord", Groups[2]),
        new("11", "Gamora", "Gamora", Groups[2]),
        new("12", "Drax", "Drax the Destroyer", Groups[2]),
        new("13", "Rocket Raccoon", "Rocket", Groups[2]),
        new("14", "Groot", "Groot", Groups[2]),
    
        // Mystics and Magicians
        new("15", "Stephen Strange", "Doctor Strange", Groups[3]),
        new("16", "Wanda Maximoff", "Scarlet Witch", Groups[3]),
        new("17", "Loki Laufeyson", "Loki", Groups[3]),
        new("18", "Wong", "Wong", Groups[3]),
    
        // Anti-Heroes
        new("19", "Wade Wilson", "Deadpool", Groups[4]),
        new("20", "Logan Howlett", "Wolverine", Groups[4]),
        new("21", "Eric Lehnsherr", "Magneto", Groups[4]),
        new("22", "Thaddeus Ross", "Red Hulk", Groups[4]),
        new("23", "Emil Blonsky", "Abomination", Groups[4])
    ];
}