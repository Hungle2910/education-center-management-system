using EducationCenter.Crm.Application.Common;
using EducationCenter.Crm.Domain.Classes;
using FluentValidation;

namespace EducationCenter.Crm.Application.CoreData.Validation;

public sealed class ClassRequestValidator : AbstractValidator<ClassRequest>
{
    public ClassRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.RequiredClassName)
            .MaximumLength(200);

        RuleFor(request => request.Subject)
            .MaximumLength(150);

        RuleFor(request => request.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || ClassStatuses.All.Contains(status))
            .WithMessage("Trạng thái lớp học không hợp lệ.");

        RuleFor(request => request.MinStudents)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sĩ số tối thiểu không hợp lệ.");

        RuleFor(request => request.MaxStudents)
            .GreaterThanOrEqualTo(request => request.MinStudents)
            .WithMessage("Sĩ số tối đa phải lớn hơn hoặc bằng sĩ số tối thiểu.");
    }
}
