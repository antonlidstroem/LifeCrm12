namespace LifeCrm.Application.Common.Exceptions;
public record ValidationError(string Field, string Message);
