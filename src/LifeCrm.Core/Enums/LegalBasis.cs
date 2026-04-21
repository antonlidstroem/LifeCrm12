namespace LifeCrm.Core.Enums;

public enum LegalBasis
{
    Consent            = 1,  // GDPR Art. 6(1)(a)
    Contract           = 2,  // GDPR Art. 6(1)(b) — e.g. donation receipt
    LegitimateInterest = 3,  // GDPR Art. 6(1)(f)
    LegalObligation    = 4   // GDPR Art. 6(1)(c)
}
