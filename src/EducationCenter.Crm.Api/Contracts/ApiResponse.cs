namespace EducationCenter.Crm.Api.Contracts;

public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data = default,
    IReadOnlyCollection<string>? Errors = null,
    string? TraceId = null)
{
    public static ApiResponse<T> Ok(T? data, string message = "Thành công.")
    {
        return new ApiResponse<T>(true, message, data);
    }

    public static ApiResponse<T> Fail(
        string message,
        IReadOnlyCollection<string>? errors = null,
        string? traceId = null)
    {
        return new ApiResponse<T>(false, message, default, errors, traceId);
    }
}
