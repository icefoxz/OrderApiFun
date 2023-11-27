namespace WebUtlLib;

public struct ResultOf
{
    public static ResultOf<T> Success<T>(T? data) => new(true, data, string.Empty);
    public static ResultOf<T> Success<T>(T? data, string message) => new(true, data, message);
    public static ResultOf<T> Fail<T>(string message) => new(false, default, message);
    public static ResultOf<T> Fail<T>(T? data, string message) => new(false, data, message);
}

public struct ResultOf<T> 
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public ResultOf(bool isSuccess, T? data,string message)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
    }
}