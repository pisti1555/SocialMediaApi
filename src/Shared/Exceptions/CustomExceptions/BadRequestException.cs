namespace Shared.Exceptions.CustomExceptions;

public class BadRequestException(string message) : AppException(400, message);