
public interface IResult
{
    bool IsSuccessful();
    string Message { get; }
}


public class Result : IResult
{
    private bool _success;
    private string _message;

    public bool IsSuccessful() => _success;

    public string Message => _message;

    public static IResult Success() => new Result 
    { 
        _success = true,
        _message = string.Empty
    };

    public static IResult Error(string message) => new Result
    {
        _message = message,
        _success = false
    };
}