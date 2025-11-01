using AvengersComms.Models;

namespace AvengersComms.Services;

public class UserService
{
    public SuperHero? CurrentUser { get; private set; }
    
    public void SetCurrentUser(SuperHero user)
    {
        CurrentUser = user;
    }
    
    public void ClearCurrentUser()
    {
        CurrentUser = null;
    }
    
    public bool IsLoggedIn => CurrentUser != null;
}