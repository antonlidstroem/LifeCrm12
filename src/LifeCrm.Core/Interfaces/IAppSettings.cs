namespace LifeCrm.Core.Interfaces;

/// <summary>
/// Thin abstraction over configuration values needed by the Application layer.
/// Implemented in Infrastructure so Application never depends on IConfiguration.
/// </summary>
public interface IAppSettings
{
    string AppBaseUrl { get; }
    string JwtSecretKey { get; }
}
