using EducationCenter.Crm.Application.Common;
using EducationCenter.Crm.Application.Common.PhoneNumbers;
using FluentValidation;

namespace EducationCenter.Crm.Application.CoreData.Validation;

public sealed class TeacherRequestValidator : AbstractValidator<TeacherRequest>
{
    public TeacherRequestValidator(IPhoneNumberNormalizer phoneNumberNormalizer)
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
            .Must(phone => string.IsNullOrWhiteSpace(phone) || phoneNumberNormalizer.TryNormalize(phone, out _))
            .WithMessage(ValidationMessages.InvalidPhoneNumber);

        RuleFor(request => request.Subject)
            .MaximumLength(150);
    }
}
