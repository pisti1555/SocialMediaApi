namespace Infrastructure.Common.Exceptions;

public class ConfigurationException(string message) : InternalException("Configuration error.", message);