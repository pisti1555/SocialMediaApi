namespace Application.Common.Results;

public class AppResult
{
    public bool Succeeded { get; protected init; }
    public List<string> Errors { get; protected init; }

    protected AppResult(bool succeeded, List<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }
    
    public static AppResult Success()
    {
        return new AppResult(true, []);
    }

    public static AppResult Failure(List<string> errors)
    {
        return new AppResult(false, errors);
    }
}

public class AppResult<T> : AppResult
{
    public T Data { get; private set; }

    private AppResult(bool succeeded, List<string> errors, T data) : base(succeeded, errors)
    {
        Succeeded = succeeded;
        Errors = errors;
        Data = data;
    }

    public static AppResult<T> Success(T data)
    {
        return new AppResult<T>(true, [], data);
    }
}