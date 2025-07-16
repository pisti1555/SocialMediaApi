namespace Shared.Exceptions.CustomExceptions;

public class NotFoundException(string message) : AppException(404, message);