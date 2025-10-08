namespace Application.Contracts.Services;

public interface IHasher
{
    public string CreateHash(string value);
}