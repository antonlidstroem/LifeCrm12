using LifeCrm.Core.Entities;

namespace LifeCrm.Core.Interfaces;

public interface IPdfService
{
    Task<byte[]> GenerateDonationReceiptAsync(Donation donation, Organization org, string receiptNumber);
    Task<byte[]> GenerateDonationSummaryAsync(Contact contact, IEnumerable<Donation> donations,
        Organization org, DateOnly from, DateOnly to);
}
