namespace Infrastructure.Common.Exceptions;

public class IdentityOperationException(string message) : InternalException("Identity operation error.", message);