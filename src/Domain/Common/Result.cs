namespace HySite.Domain.Common;

public class Result<T>
{
    private bool _success;
    private string? _message;
    private T? _value { get; set; }

    public bool IsSuccessful => _success;
    public string Message => _message ?? string.Empty;
    public T Value => _value!;

    public static Result<T> Success(T value) => new Result<T>
    { 
        _success = true,
        _message = string.Empty,
        _value = value
    };

    public static Result<T> Error(string message) => new Result<T>
    {
        _message = message,
        _success = false
    };
}