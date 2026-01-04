using CoShare.Api.Contracts.Auth;
using FluentValidation;

namespace CoShare.Api.Validators;

/// <summary>
/// Validator for EndUserRegisterStartRequest per OpenAPI and US-AUTH-001 edge cases.
/// </summary>
public class EndUserRegisterStartRequestValidator : AbstractValidator<EndUserRegisterStartRequest>
{
    public EndUserRegisterStartRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("Company ID must be a positive number.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .Matches(@"^0\d{9,10}$").WithMessage("Phone number format is invalid.");

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("You must accept the terms and conditions.");
    }
}

/// <summary>
/// Validator for EndUserRegisterVerifyRequest per OpenAPI.
/// </summary>
public class EndUserRegisterVerifyRequestValidator : AbstractValidator<EndUserRegisterVerifyRequest>
{
    public EndUserRegisterVerifyRequestValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .Matches(@"^0\d{9,10}$").WithMessage("Phone number format is invalid.");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("OTP code is required.")
            .MaximumLength(10).WithMessage("OTP code must not exceed 10 characters.")
            .Matches(@"^\d+$").WithMessage("OTP code must contain only digits.");
    }
}
