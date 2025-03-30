namespace TeachTether.Application.Common
{
    public class OperationResult
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; } = new();

        public static OperationResult Success() => new() { Succeeded = true };

        public static OperationResult Failure(IEnumerable<string> errors) =>
            new() { Succeeded = false, Errors = errors.ToList() };
    }

}
