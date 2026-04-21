using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LifeCrm.Infrastructure.Services;

public class PdfService : IPdfService
{
    static PdfService() { QuestPDF.Settings.License = LicenseType.Community; }

    public async Task<byte[]> GenerateDonationReceiptAsync(
        Donation donation, Organization org, string receiptNumber)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Text(org.Name).Bold().FontSize(18);
                    row.ConstantItem(150).AlignRight().Text($"Receipt #{receiptNumber}").FontSize(10);
                });

                page.Content().PaddingTop(20).Column(col =>
                {
                    col.Item().Text($"Date: {donation.Date:yyyy-MM-dd}");
                    col.Item().Text($"Amount: {donation.Amount:C}");
                    col.Item().Text($"Status: {donation.Status}");
                    if (!string.IsNullOrEmpty(donation.ReferenceNumber))
                        col.Item().Text($"Reference: {donation.ReferenceNumber}");
                    col.Item().PaddingTop(20).Text("Thank you for your generous contribution.");
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages();
                });
            });
        });

        return await Task.FromResult(doc.GeneratePdf());
    }

    public async Task<byte[]> GenerateDonationSummaryAsync(
        Contact contact, IEnumerable<Donation> donations, Organization org,
        DateOnly from, DateOnly to)
    {
        var donList = donations.ToList();
        var total   = donList.Sum(d => d.Amount);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Text(org.Name).Bold().FontSize(18);
                    row.ConstantItem(180).AlignRight().Text("Donation Summary").FontSize(12).Bold();
                });

                page.Content().PaddingTop(20).Column(col =>
                {
                    col.Item().Text($"Donor: {contact.FullName}");
                    col.Item().Text($"Period: {from:yyyy-MM-dd} – {to:yyyy-MM-dd}");
                    col.Item().PaddingTop(16).Table(table =>
                    {
                        table.ColumnsDefinition(c => {
                            c.ConstantColumn(100); c.RelativeColumn(); c.ConstantColumn(100);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Text("Date").Bold();
                            h.Cell().Text("Description").Bold();
                            h.Cell().AlignRight().Text("Amount").Bold();
                        });
                        foreach (var d in donList)
                        {
                            table.Cell().Text(d.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Text(d.Campaign?.Name ?? d.Project?.Name ?? "General");
                            table.Cell().AlignRight().Text($"{d.Amount:C}");
                        }
                    });
                    col.Item().PaddingTop(10).AlignRight().Text($"Total: {total:C}").Bold().FontSize(13);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages();
                });
            });
        });

        return await Task.FromResult(doc.GeneratePdf());
    }
}
