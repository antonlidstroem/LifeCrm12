namespace LifeCrm.Application.Common.Exceptions;
public class ValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }
    public ValidationException(IEnumerable<ValidationError> errors) : base("One or more validation errors occurred.")
    { Errors = errors.ToList().AsReadOnly(); }
    public ValidationException(string field, string message) : this(new[] { new ValidationError(field, message) }) { }
}
