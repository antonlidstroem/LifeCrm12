using FluentValidation;
using LifeCrm.Application.Campaigns.DTOs;

namespace LifeCrm.Application.Campaigns.Commands;

public sealed class CreateCampaignValidator : AbstractValidator<CreateCampaignRequest>
{
    public CreateCampaignValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BudgetGoal).GreaterThanOrEqualTo(0).When(x => x.BudgetGoal.HasValue);
        // Every campaign must belong to a project
        RuleFor(x => x.ProjectId)
            .NotNull().WithMessage("A campaign must be linked to a project.")
            .NotEqual(Guid.Empty).WithMessage("A campaign must be linked to a project.");
    }
}

public sealed class UpdateCampaignValidator : AbstractValidator<UpdateCampaignRequest>
{
    public UpdateCampaignValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        Include(new CreateCampaignValidator());
    }
}
