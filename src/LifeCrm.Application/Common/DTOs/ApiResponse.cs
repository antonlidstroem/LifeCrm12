namespace LifeCrm.Application.Common.DTOs;

public class ApiResponse
{
    public bool Success { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static ApiResponse Ok() => new() { Success = true };
    public static ApiResponse Fail(params string[] errors) => new() { Success = false, Errors = errors };
    public static ApiResponse Fail(IEnumerable<string> errors) => new() { Success = false, Errors = errors.ToList() };
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public new static ApiResponse<T> Fail(params string[] errors) =>
        new() { Success = false, Errors = errors };
}
