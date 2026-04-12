namespace LifeCrm.Core.Interfaces;

public interface IUnsubscribeTokenService
{
    string GenerateToken(Guid contactId, Guid organizationId);
}
