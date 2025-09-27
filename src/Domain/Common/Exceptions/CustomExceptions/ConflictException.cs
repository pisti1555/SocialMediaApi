namespace Domain.Common.Exceptions.CustomExceptions;

public class ConflictException(string message) : AppException(409, message);