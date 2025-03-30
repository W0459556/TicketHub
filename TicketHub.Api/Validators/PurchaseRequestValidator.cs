using FluentValidation;
using TicketHub.Api.Models;

namespace TicketHub.Api.Validators
{
    public class PurchaseRequestValidator : AbstractValidator<PurchaseRequest>
    {
        public PurchaseRequestValidator()
        {
            RuleFor(x => x.ConcertId).GreaterThan(0);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.CreditCard).NotEmpty().CreditCard();
            RuleFor(x => x.Expiration).NotEmpty().Matches(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$");
            RuleFor(x => x.SecurityCode).NotEmpty().Length(3, 4);
            RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Province).NotEmpty().MaximumLength(100);
            RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        }
    }
}