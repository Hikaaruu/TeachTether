namespace TeachTether.Application.Common;

public class OperationResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = [];

    public static OperationResult Success()
    {
        return new OperationResult { Succeeded = true };
    }

    public static OperationResult Failure(IEnumerable<string> errors)
    {
        return new OperationResult { Succeeded = false, Errors = [.. errors] };
    }
}