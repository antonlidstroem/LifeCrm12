// src/LifeCrm.Application/Dashboard/GetDashboardQuery.cs
using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Dashboard;

public class GetDashboardQuery : IRequest<DashboardDto> { }

public sealed class GetDashboardHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IUnitOfWork _uow;
    public GetDashboardHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<DashboardDto> Handle(GetDashboardQuery q, CancellationToken ct)
    {
        var today          = DateOnly.FromDateTime(DateTime.UtcNow);
        var thisMonthStart = new DateOnly(today.Year, today.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        var lastMonthEnd   = thisMonthStart.AddDays(-1);
        var thisMonthStartDto = new DateTimeOffset(
            thisMonthStart.Year, thisMonthStart.Month, 1, 0, 0, 0, TimeSpan.Zero);

        // FIX C: Run all independent aggregate queries in parallel instead of
        // sequentially. Previous code awaited each query one-at-a-time, causing
        // 8+ serial round-trips to the database. Task.WhenAll issues them
        // concurrently, reducing total latency to ~the slowest single query.

        var thisMonthTask = _uow.Donations.Query()
            .Where(d => d.Date >= thisMonthStart)
            .SumAsync(d => (decimal?)d.Amount, ct);

        var lastMonthTask = _uow.Donations.Query()
            .Where(d => d.Date >= lastMonthStart && d.Date <= lastMonthEnd)
            .SumAsync(d => (decimal?)d.Amount, ct);

        var contactsTotalTask = _uow.Contacts.CountAsync(_ => true, ct);

        var newContactsTask = _uow.Contacts.CountAsync(
            c => c.CreatedAt >= thisMonthStartDto, ct);

        var recentDonationsTask = _uow.Donations.Query()
            .OrderByDescending(d => d.CreatedAt).Take(10)
            .Select(d => new ActivityFeedItemDto
            {
                ActivityType = "Donation",
                EntityId     = d.Id,
                ContactId    = d.ContactId,
                ContactName  = d.Contact != null ? d.Contact.Name : string.Empty,
                Summary      = "$" + d.Amount.ToString("N2") + " donation",
                OccurredAt   = d.CreatedAt
            }).ToListAsync(ct);

        var recentInteractionsTask = _uow.Interactions.Query()
            .Where(i => i.ContactId.HasValue)
            .OrderByDescending(i => i.OccurredAt).Take(10)
            .Select(i => new ActivityFeedItemDto
            {
                ActivityType = "Interaction",
                EntityId     = i.Id,
                ContactId    = i.ContactId!.Value,
                ContactName  = i.Contact != null ? i.Contact.Name : string.Empty,
                Summary      = i.Type.ToString() + " logged",
                OccurredAt   = i.OccurredAt
            }).ToListAsync(ct);

        // Active campaigns + their donation totals are sequential because we
        // need the campaign IDs first. But we overlap them with the queries above.
        var activeCampaignsTask = _uow.Campaigns.GetActiveAsync(ct);

        // Await all parallel tasks together
        await Task.WhenAll(
            thisMonthTask,
            lastMonthTask,
            contactsTotalTask,
            newContactsTask,
            recentDonationsTask,
            recentInteractionsTask,
            activeCampaignsTask);

        var thisMonth  = thisMonthTask.Result ?? 0;
        var lastMonth  = lastMonthTask.Result ?? 0;
        var contacts   = contactsTotalTask.Result;
        var newCon     = newContactsTask.Result;
        var recentDon  = recentDonationsTask.Result;
        var recentInt  = recentInteractionsTask.Result;
        var active     = activeCampaignsTask.Result;

        var moM = lastMonth > 0
            ? Math.Round((thisMonth - lastMonth) / lastMonth * 100, 1)
            : 0;

        // Campaign totals — one query for all active campaigns
        var ids = active.Select(c => c.Id).ToList();
        var totals = ids.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await _uow.Donations.Query()
                .Where(d => d.CampaignId.HasValue && ids.Contains(d.CampaignId.Value))
                .GroupBy(d => d.CampaignId!.Value)
                .Select(g => new { g.Key, Total = g.Sum(d => (decimal?)d.Amount) ?? 0 })
                .ToDictionaryAsync(x => x.Key, x => x.Total, ct);

        var top = active.Take(5).Select(c =>
        {
            var r = totals.GetValueOrDefault(c.Id, 0m);
            return new CampaignSummaryDto
            {
                Id              = c.Id,
                Name            = c.Name,
                BudgetGoal      = c.BudgetGoal,
                TotalRaised     = r,
                ProgressPercent = c.BudgetGoal.HasValue && c.BudgetGoal > 0
                    ? Math.Round(r / c.BudgetGoal.Value * 100, 1) : null
            };
        }).ToList().AsReadOnly();

        var feed = recentDon.Concat(recentInt)
            .OrderByDescending(a => a.OccurredAt)
            .Take(10).ToList().AsReadOnly();

        return new DashboardDto
        {
            DonationsThisMonth        = thisMonth,
            DonationsLastMonth        = lastMonth,
            DonationsMoMChangePercent = moM,
            TotalContacts             = contacts,
            NewContactsThisMonth      = newCon,
            ActiveCampaigns           = active.Count,
            TopCampaigns              = top,
            RecentActivity            = feed
        };
    }
}
