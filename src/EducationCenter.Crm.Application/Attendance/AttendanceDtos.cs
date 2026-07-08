using System;
using System.Collections.Generic;

namespace EducationCenter.Crm.Application.Attendance;

public sealed record StudentAttendanceDto(
    Guid StudentId,
    string StudentName,
    string Status, // Có mặt, Vắng có phép, Vắng không phép, Đi trễ, Đã học bù
    string? Notes
);

public sealed record OccurrenceAttendanceDto(
    Guid OccurrenceId,
    string ClassName,
    string Date,
    string StartTime,
    string EndTime,
    string Status, // Buổi học status: Đã lên lịch, Đã học, Đã nghỉ, Đã hủy
    List<StudentAttendanceDto> Students
);

public sealed record SubmitAttendanceRequest(
    Guid OccurrenceId,
    List<StudentAttendanceDto> Students
);
