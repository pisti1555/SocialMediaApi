namespace Infrastructure.Auth.Exceptions;

public class JwtException(string message) : Exception(message);