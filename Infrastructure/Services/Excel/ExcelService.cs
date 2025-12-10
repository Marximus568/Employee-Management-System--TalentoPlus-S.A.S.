using Application.Interfaces.Excel;
using Domain.Entities;
using Domain.Models;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Infrastructure.Services.Excel;

public class ExcelService : IExcelService
{
    private readonly ApplicationDbContext _context;

    public ExcelService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ImportEmployeesAsync(Stream fileStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null || worksheet.Dimension == null) return;

        var rowCount = worksheet.Dimension.Rows;
        var colCount = worksheet.Dimension.Columns;

        // 1. Map headers
        var headerMap = new Dictionary<string, int>();
        for (int col = 1; col <= colCount; col++)
        {
            var header = worksheet.Cells[1, col].Text.Trim().ToLower();
            if(!string.IsNullOrEmpty(header)) headerMap[header] = col;
        }

        // 2. Read all rows into memory to avoid repeated DB calls and handle duplicates within file
        var importData = new List<ImportRow>();
        for (int row = 2; row <= rowCount; row++)
        {
            var doc = GetValue(worksheet, row, headerMap, "documento", "document", "cedula", "identificacion");
            if (string.IsNullOrEmpty(doc)) continue;
            
            importData.Add(new ImportRow
            {
                RowIndex = row,
                Document = doc,
                FirstName = GetValue(worksheet, row, headerMap, "nombre", "nombres", "firstname") ?? "Unknown",
                LastName = GetValue(worksheet, row, headerMap, "apellido", "apellidos", "lastname") ?? "Unknown",
                Email = GetValue(worksheet, row, headerMap, "correo", "email", "mail"),
                Phone = GetValue(worksheet, row, headerMap, "telefono", "celular", "phone"),
                Address = GetValue(worksheet, row, headerMap, "direccion", "address"),
                Department = GetValue(worksheet, row, headerMap, "departamento", "department", "area"),
                Salary = GetValue(worksheet, row, headerMap, "salario", "salary"),
                Position = GetValue(worksheet, row, headerMap, "cargo", "position", "rol"),
                HireDate = GetValue(worksheet, row, headerMap, "fechaingreso", "hiredate", "fecha de ingreso"),
                BirthDate = GetValue(worksheet, row, headerMap, "fechanacimiento", "birthdate", "fecha de nacimiento"),
                Status = GetValue(worksheet, row, headerMap, "estado", "status"),
                EducationLevel = GetValue(worksheet, row, headerMap, "niveleducativo", "educationlevel", "education", "nivel educativo"),
                ProfessionalProfile = GetValue(worksheet, row, headerMap, "perfilprofesional", "professionalprofile", "profile", "perfil profesional")
            });
        }

        if(!importData.Any()) return;

        // 3. Bulk Fetch Existing Data
        var documents = importData.Select(d => d.Document).Distinct().ToList();
        
        // We query Employees directly to ensure we get the derived type data if available
        // But we really need to check if ANY Person with that document exists.
        var existingPersons = await _context.Persons
            .Include(p => (p as Employee).EducationRecords)
            .Where(p => documents.Contains(p.Document))
            .ToDictionaryAsync(p => p.Document);

        var allDepartmentNames = importData.Select(d => d.Department).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
        
        // Fetch departments and create a case-insensitive dictionary
        var dbDepartments = await _context.Departments.ToListAsync();
        var existingDepartments = new Dictionary<string, Department>(StringComparer.OrdinalIgnoreCase);
        foreach (var dept in dbDepartments)
        {
            if (!existingDepartments.ContainsKey(dept.Name))
            {
                existingDepartments[dept.Name] = dept;
            }
        }

        // 4. Process Rows
        var newDepartments = new Dictionary<string, Department>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in importData)
        {
            // Resolve Department
            Department? department = null;
            if (!string.IsNullOrEmpty(row.Department))
            {
                if (existingDepartments.TryGetValue(row.Department, out var dept))
                {
                    department = dept;
                }
                else if (newDepartments.TryGetValue(row.Department, out var newDept))
                {
                    department = newDept;
                }
                else
                {
                    department = new Department { Name = row.Department }; // Description removed
                    newDepartments[row.Department] = department;
                    _context.Departments.Add(department);
                }
            }

            // Resolve Person / Employee
             Employee employee;

            if (!existingPersons.TryGetValue(row.Document, out var person))
            {
                // Create NEW Employee (since Person is abstract)
                employee = new Employee
                {
                    Document = row.Document,
                    FirstName = row.FirstName,
                    LastName = row.LastName,
                    Email = row.Email,
                    Phone = row.Phone,
                    Address = row.Address,
                    Status = !string.IsNullOrEmpty(row.Status) ? row.Status : "Active",
                    HireDate = DateOnly.FromDateTime(DateTime.Now),
                    EducationRecords = new List<EmployeeEducation>()
                };
                
                _context.Employees.Add(employee);
                existingPersons[row.Document] = employee; // Track
            }
            else
            {
                // Update Existing
                if (!string.IsNullOrEmpty(row.Email)) person.Email = row.Email;
                if (!string.IsNullOrEmpty(row.Phone)) person.Phone = row.Phone;
                if (!string.IsNullOrEmpty(row.Address)) person.Address = row.Address;
                
                if (person is Employee emp)
                {
                    employee = emp;
                }
                else
                {
                    // Person exists but is NOT an employee (e.g. Client). 
                    // Cannot easily convert. Skip Employee-specific updates or log warning.
                    // For now, continue to next row or skip employee specific logic
                    continue; 
                }
            }

            // Update Employee fields
            if (decimal.TryParse(row.Salary, out var sal)) employee.Salary = sal;
            if (!string.IsNullOrEmpty(row.Position)) employee.Position = row.Position;
            if (!string.IsNullOrEmpty(row.Status)) employee.Status = row.Status;
            
            if (DateTime.TryParse(row.HireDate, out var hd)) 
                employee.HireDate = DateOnly.FromDateTime(hd);
            
            if (DateTime.TryParse(row.BirthDate, out var bd)) 
                employee.BirthDate = DateOnly.FromDateTime(bd);

            if (department != null) employee.Department = department;

            // Resolve Education
            if (!string.IsNullOrEmpty(row.EducationLevel))
            {
                if (employee.EducationRecords == null) employee.EducationRecords = new List<EmployeeEducation>();

                var education = employee.EducationRecords
                    .FirstOrDefault(e => e.EducationLevel == row.EducationLevel);

                if (education == null)
                {
                    employee.EducationRecords.Add(new EmployeeEducation
                    {
                        EducationLevel = row.EducationLevel,
                        ProfessionalProfile = row.ProfessionalProfile ?? "Not Specified"
                    });
                }
                else
                {
                    if (!string.IsNullOrEmpty(row.ProfessionalProfile))
                        education.ProfessionalProfile = row.ProfessionalProfile;
                }
            }
            
            // Also update the main ProfessionalProfile on Employee if present
            if (!string.IsNullOrEmpty(row.ProfessionalProfile))
            {
                employee.ProfessionalProfile = row.ProfessionalProfile;
            }
        }

        await _context.SaveChangesAsync();
    }

    private class ImportRow
    {
        public int RowIndex { get; set; }
        public string Document { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Department { get; set; }
        public string? Salary { get; set; }
        public string? Position { get; set; }
        public string? HireDate { get; set; }
        public string? BirthDate { get; set; }
        public string? Status { get; set; }
        public string? EducationLevel { get; set; }
        public string? ProfessionalProfile { get; set; }
    }

    private string? GetValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> map, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (map.TryGetValue(key, out int col))
            {
                return worksheet.Cells[row, col].Text?.Trim();
            }
        }
        return null; // Not found
    }
}
