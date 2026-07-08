using EducationCenter.Crm.Application.Common;
using EducationCenter.Crm.Application.Common.PhoneNumbers;
using FluentValidation;

namespace EducationCenter.Crm.Application.CoreData.Validation;

public sealed class ParentRequestValidator : AbstractValidator<ParentRequest>
{
    public ParentRequestValidator(IPhoneNumberNormalizer phoneNumberNormalizer)
    {
        RuleFor(request => request.FullName)
            .NotEmpty()
            .WithMessage(ValidationMessages.RequiredFullName)
            .MaximumLength(200);

        RuleFor(request => request.Email)
            .EmailAddress()
            .WithMessage(ValidationMessages.InvalidEmail)
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.PhoneNumber)
            .Must(phone => phoneNumberNormalizer.TryNormalize(phone, out _))
            .WithMessage(ValidationMessages.InvalidPhoneNumber);
    }
}
