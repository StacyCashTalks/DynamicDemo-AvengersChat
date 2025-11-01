using AvengersComms.Data;

namespace AvengersComms.Services;

public class MessagesService(UserService userService)
{
    public void SendToAllAsync()
    {
        if (userService.CurrentUser is null)
        {
            return;
        }
        
        var messageTexts = 
            HeroMessages.Messages
                .Where(m => m.FromHeroId.ToString() == userService.CurrentUser.Id)
                .ToList();
        var messageText = messageTexts.Count == 0 
            ? RandomMessageGenerator() 
            : messageTexts.ElementAt(new Random().Next(messageTexts.Count)).Message;
    }

    public void SendToGroupAsync()
    {
        if (userService.CurrentUser is null)
        {
            return;
        }
        
        var messageTexts = 
            HeroMessages.Messages
                .Where(m => 
                    m.FromHeroId.ToString() == userService.CurrentUser.Id
                    && m.ToHeroId.ToString() == userService.CurrentUser.Group.Id 
                    && m.Category == "group")
                .ToList();
        var messageText = messageTexts.Count == 0 
            ? RandomMessageGenerator() 
            : messageTexts.ElementAt(new Random().Next(messageTexts.Count)).Message;
        
    }

    public void SendToHeroAsync(string toPersonId)
    {
        if (userService.CurrentUser is null)
        {
            return;
        }
        
        var messageTexts = 
            HeroMessages.Messages
                .Where(m => 
                    m.FromHeroId.ToString() == userService.CurrentUser.Id
                    && m.ToHeroId.ToString() == toPersonId 
                    && m.Category == "direct")
                .ToList();
        string messageText;
        messageText = messageTexts.Count == 0 
            ? RandomMessageGenerator() 
            : messageTexts.ElementAt(new Random().Next(messageTexts.Count)).Message;
        
    }
    
    private static string RandomMessageGenerator()
    {
        // Create a random message from fake static, to I got nothing to say to you, to something just bizar
        var fallbackMessages = new List<string>
        {
            "I got nothing to say to you.",
            "zzzzzzzz <static> xcxcxcxcxc <crackle> zzzzzzz",
            "If you stir pancake batter too long, the bananas get offended.",
            "Sorry, I’m busy rearranging my sock drawer alphabetically.",
            "Once I tried to bake a cake in the mailbox, results were inconclusive.",
            "Did you know rubber ducks can't whistle in space?",
            "<silence> ... ... ... <static>"
        };
        var randomIndex = new Random().Next(fallbackMessages.Count);
        var randomMessage = fallbackMessages[randomIndex];
        return randomMessage;
    }
}
