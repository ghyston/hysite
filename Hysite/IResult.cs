
public interface IResult
{
    bool IsSuccess();
    string Message { get; }
}


public class Result : IResult
{
    private bool _success;
    private string _message;

    public bool IsSuccess() => _success;

    public string Message => _message;

    static IResult Success() => new Result 
    { 
        _success = true,
        _message = string.Empty
    };

    static IResult Error(string message) => new Result
    {
        _message = message,
        _success = false
    };
}