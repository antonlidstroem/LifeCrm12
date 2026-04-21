namespace LifeCrm.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.") { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Access denied.") : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }
    public ValidationException(string field, string message)
        : base($"{field}: {message}")
    {
        Errors = new[] { new ValidationError(field, message) };
    }
    public ValidationException(IEnumerable<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList();
    }
}

public record ValidationError(string Field, string Message);
