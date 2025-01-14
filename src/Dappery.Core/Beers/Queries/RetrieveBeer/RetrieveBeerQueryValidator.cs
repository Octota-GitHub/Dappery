using FluentValidation;

namespace Dappery.Core.Beers.Queries.RetrieveBeer;

public class RetrieveBeerQueryValidator : AbstractValidator<RetrieveBeerQuery>
{
    public RetrieveBeerQueryValidator() => this.RuleFor(b => b.Id)
        .NotEmpty()
        .WithMessage("Must supply an ID to retrieve a beer")
        .GreaterThanOrEqualTo(1)
        .WithMessage("Must be a valid beer ID");
}
