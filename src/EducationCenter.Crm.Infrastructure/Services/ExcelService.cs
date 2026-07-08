using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EducationCenter.Crm.Application.Common;
using MiniExcelLibs;

namespace EducationCenter.Crm.Infrastructure.Services;

public class ExcelService : IExcelService
{
    public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1")
    {
        using var ms = new MemoryStream();
        ms.SaveAs(data, sheetName: sheetName);
        return ms.ToArray();
    }

    public List<StudentImportRow> ParseStudentImport(Stream stream)
    {
        var list = new List<StudentImportRow>();
        
        try
        {
            var rows = stream.Query(useHeaderRow: true);
            foreach (IDictionary<string, object> row in rows)
            {
                var studentName = GetValue(row, "Họ tên học sinh");
                var parentName = GetValue(row, "Họ tên phụ huynh");
                var parentPhone = GetValue(row, "Số điện thoại phụ huynh");
                var dobStr = GetValue(row, "Ngày sinh (dd/MM/yyyy)");
                var className = GetValue(row, "Tên lớp học");

                if (!string.IsNullOrWhiteSpace(studentName))
                {
                    list.Add(new StudentImportRow
                    {
                        StudentName = studentName.Trim(),
                        ParentName = parentName.Trim(),
                        ParentPhone = parentPhone.Trim(),
                        DateOfBirthString = dobStr.Trim(),
                        ClassName = className.Trim()
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi đọc file Excel: {ex.Message}");
        }

        return list;
    }

    public byte[] GetStudentImportTemplate()
    {
        var templateData = new[]
        {
            new Dictionary<string, object>
            {
                { "Họ tên học sinh", "Nguyễn Văn A" },
                { "Họ tên phụ huynh", "Nguyễn Văn B" },
                { "Số điện thoại phụ huynh", "0909123456" },
                { "Ngày sinh (dd/MM/yyyy)", "15/08/2012" },
                { "Tên lớp học", "Toán 9A" }
            }
        };

        using var ms = new MemoryStream();
        ms.SaveAs(templateData);
        return ms.ToArray();
    }

    private string GetValue(IDictionary<string, object> row, string key)
    {
        if (row.TryGetValue(key, out var val) && val != null)
        {
            return val.ToString() ?? string.Empty;
        }
        return string.Empty;
    }
}
