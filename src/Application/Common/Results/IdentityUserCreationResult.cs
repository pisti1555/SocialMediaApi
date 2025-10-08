namespace Application.Common.Results;

public class IdentityUserCreationResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = [];
}