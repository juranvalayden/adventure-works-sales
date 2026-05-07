namespace Sales.Application.Abstractions;

public record Error(ErrorType ErrorType, string Description)
{
    public static readonly Error None = new(ErrorType.None, string.Empty);
    public static readonly Error NullValue = new(ErrorType.Null, "Null value was provided.");
    
    public static implicit operator Response(Error error) => Response.Failure(error);
 
    public Response ToResult() => Response.Failure(this);
}