using EducationCenter.Crm.Application.Common;
using EducationCenter.Crm.Application.Common.PhoneNumbers;
using EducationCenter.Crm.Domain.People;
using FluentValidation;

namespace EducationCenter.Crm.Application.CoreData.Validation;

public sealed class StudentRequestValidator : AbstractValidator<StudentRequest>
{
    public StudentRequestValidator(IPhoneNumberNormalizer phoneNumberNormalizer)
    {
        RuleFor(request => request.FullName)
            .NotEmpty()
            .WithMessage(ValidationMessages.RequiredFullName)
            .MaximumLength(200);

        RuleFor(request => request.StudentCode)
            .MaximumLength(50);

        RuleFor(request => request.Email)
            .EmailAddress()
            .WithMessage(ValidationMessages.InvalidEmail)
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.PhoneNumber)
            .Must(phone => string.IsNullOrWhiteSpace(phone) || phoneNumberNormalizer.TryNormalize(phone, out _))
            .WithMessage(ValidationMessages.InvalidPhoneNumber);

        RuleFor(request => request.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || StudentStatuses.All.Contains(status))
            .WithMessage("Trạng thái học sinh không hợp lệ.");
    }
}
