namespace Domain.Common.Exceptions.CustomExceptions;

public class ForbiddenException(string message) : AppException(403, message);