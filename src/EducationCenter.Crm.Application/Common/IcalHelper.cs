using System;
using System.Collections.Generic;
using System.Text;
using EducationCenter.Crm.Domain.Classes;

namespace EducationCenter.Crm.Application.Common;

public static class IcalHelper
{
    public static string GenerateIcalString(IEnumerable<ScheduleOccurrence> occurrences)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//Education Center CRM//Nonsg//EN");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine("METHOD:PUBLISH");

        foreach (var o in occurrences)
        {
            var dateStr = o.Date.ToString("yyyyMMdd");
            var startStr = o.StartTime.ToString("HHmmss");
            var endStr = o.EndTime.ToString("HHmmss");

            var className = o.Class?.Name ?? "Lớp học";
            var roomName = o.Room?.Name ?? "Phòng học";
            var teacherName = o.Teacher?.FullName ?? "Chưa phân công";

            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{o.Id}");
            sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            sb.AppendLine($"DTSTART;TZID=Asia/Ho_Chi_Minh:{dateStr}T{startStr}");
            sb.AppendLine($"DTEND;TZID=Asia/Ho_Chi_Minh:{dateStr}T{endStr}");
            sb.AppendLine($"SUMMARY:{className} - {roomName}");
            sb.AppendLine($"LOCATION:{roomName}");
            sb.AppendLine($"DESCRIPTION:Giáo viên: {teacherName}\\nMôn học: {o.Class?.Subject ?? "Chưa rõ"}\\nTrạng thái: {o.Status}");
            sb.AppendLine("END:VEVENT");
        }

        sb.AppendLine("END:VCALENDAR");
        return sb.ToString();
    }
}
