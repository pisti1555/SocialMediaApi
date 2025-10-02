namespace Application;

public class IdentityUserCreationResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = [];
}