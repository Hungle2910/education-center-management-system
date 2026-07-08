using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Api.Controllers;
using EducationCenter.Crm.Application.CoreData;
using EducationCenter.Crm.Application.CoreData.Validation;
using EducationCenter.Crm.Application.Common.PhoneNumbers;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Crm.Tests;

public sealed class StudentsControllerTests
{
    [Fact]
    public async Task Update_WhenStudentDoesNotExist_ReturnsNotFoundWithVietnameseMessage()
    {
        var controller = new StudentsController(
            new StudentServiceStub(),
            new StudentRequestValidator(new VietnamPhoneNumberNormalizer()));
        var request = new StudentRequest(
            "Nguyễn Văn A",
            null,
            null,
            "0909123456",
            null,
            null,
            null);

        var result = await controller.Update(Guid.NewGuid(), request, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);
        Assert.False(response.Success);
        Assert.Equal("Không tìm thấy học sinh.", response.Message);
    }

    private sealed class StudentServiceStub : IStudentService
    {
        public Task<IReadOnlyCollection<StudentResponse>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<StudentResponse>>(Array.Empty<StudentResponse>());
        }

        public Task<StudentResponse> CreateAsync(
            StudentRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<StudentResponse?> UpdateAsync(
            Guid id,
            StudentRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<StudentResponse?>(null);
        }

        public Task<StudentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult<StudentResponse?>(null);
        }

        public Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult<(bool Success, string? Error)>((true, null));
        }
    }
}
