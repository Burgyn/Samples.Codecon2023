using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser
{
    public string Type { get; set; } = "basic";
}
