namespace Infrastructure.Common.Exceptions;

public class PersistenceException(string message) : InternalException("Persistence error.", message);