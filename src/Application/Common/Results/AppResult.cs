namespace Application.Common.Results;

public class AppResult
{
    public bool Succeeded { get; protected init; }
    public List<string> Errors { get; protected init; }

    protected AppResult() { }
    
    public static AppResult Success()
    {
        return new AppResult
        {
            Succeeded = true,
            Errors = []
        };
    }

    public static AppResult Failure(List<string> errors)
    {
        return new AppResult
        {
            Succeeded = false,
            Errors = errors
        };
    }
}

public class AppResult<T> : AppResult
{
    public T Data { get; private set; }

    private AppResult() { }

    public static AppResult<T> Success(T data)
    {
        return new AppResult<T>
        {
            Succeeded = true,
            Errors = [],
            Data = data
        };
    }
    
    public new static AppResult<T> Failure(List<string> errors)
    {
        return new AppResult<T>
        {
            Succeeded = false,
            Errors = errors
        };
    }
}