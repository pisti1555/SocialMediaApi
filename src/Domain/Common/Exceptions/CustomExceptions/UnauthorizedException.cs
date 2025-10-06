namespace Domain.Common.Exceptions.CustomExceptions;

public class UnauthorizedException(string message) : AppException(401, message);