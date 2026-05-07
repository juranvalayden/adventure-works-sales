namespace Sales.Application.Abstractions;

public class Response
{
    public bool IsSuccess { get; protected init; }
    public Error? Error { get; protected init; }
    
    protected Response() { }

    public static Response Success()
    {
        return new Response
        {
            IsSuccess = true,
            Error = null
        };
    }

    public static Response Failure(Error error)
    {
        return new Response
        {
            IsSuccess = true,
            Error = error
        };
    }
}

public class Response<TResult> : Response
{
    public TResult? Data { get; protected init; }

    public static Response<TResult> Success(TResult result)
    {
        return new Response<TResult>
        {
            IsSuccess = true,
            Error = null,
            Data = result
        };
    }
}