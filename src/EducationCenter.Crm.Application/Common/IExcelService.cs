using System.Collections.Generic;
using System.IO;

namespace EducationCenter.Crm.Application.Common;

public interface IExcelService
{
    byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1");
    
    List<StudentImportRow> ParseStudentImport(Stream stream);
    
    byte[] GetStudentImportTemplate();
}

public class StudentImportRow
{
    public string StudentName { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string ParentPhone { get; set; } = string.Empty;
    public string DateOfBirthString { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
}
