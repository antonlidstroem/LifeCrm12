namespace LifeCrm.Application.Common.Exceptions;
public class NotFoundException : Exception
{
    public string EntityName { get; }
    public object Key { get; }
    public NotFoundException(string entityName, object key) : base($"{entityName} with ID '{key}' was not found.")
    { EntityName = entityName; Key = key; }
}
