namespace SharedKernel.DTOs;

public class Result
{
    public string? Error { get;  }
    public bool IsSuccess { get; }
    public bool IsFailure  => !IsSuccess;
    
    protected Result(bool isSuccess,string? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException();

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException();
        
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success() => new Result(true,null);
    public static Result Failure(string error) => new Result(false,error);
}

public class Result<T> : Result
{
    public T? Value { get; set; }
    
    protected Result(T value,bool isSuccess,string? error):base(isSuccess,error){
        Value = value;
    }
    
    public static Result<T> Success(T value) => new(value,true,null);

    public new static Result<T> Failure(string error) => new(default!,false,error);
}