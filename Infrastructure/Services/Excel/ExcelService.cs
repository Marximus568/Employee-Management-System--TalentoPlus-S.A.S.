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

        // 2. Read all rows into memory
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
                HireDate = GetValue(worksheet, row, headerMap, "fechaingreso", "hiredate", "fecha de ingreso", "fechaingre"),
                BirthDate = GetValue(worksheet, row, headerMap, "fechanacimiento", "birthdate", "fecha de nacimiento"),
                Status = GetValue(worksheet, row, headerMap, "estado", "status"),
                EducationLevel = GetValue(worksheet, row, headerMap, "niveleducativo", "educationlevel", "education", "nivel educativo"),
                ProfessionalProfile = GetValue(worksheet, row, headerMap, "perfilprofesional", "professionalprofile", "profile", "perfil profesional")
            });
        }

        if(!importData.Any()) return;

        // 3. Process Departments FIRST (Save them to get IDs)
        var distinctDeptNames = importData
            .Select(d => d.Department)
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingDepartments = await _context.Departments
            .Where(d => distinctDeptNames.Contains(d.Name))
            .ToListAsync();
            
        var deptMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in existingDepartments) deptMap[d.Name] = d.Id;

        var newDepartments = new List<Department>();
        foreach (var name in distinctDeptNames)
        {
            if (!deptMap.ContainsKey(name))
            {
                var newDept = new Department { Name = name };
                newDepartments.Add(newDept);
            }
        }

        if (newDepartments.Any())
        {
            _context.Departments.AddRange(newDepartments);
            await _context.SaveChangesAsync(); // Save to generate IDs
            
            foreach (var d in newDepartments) deptMap[d.Name] = d.Id;
        }

        // 4. Process Employees
        var documents = importData.Select(d => d.Document).Distinct().ToList();
        var existingEmployees = await _context.Employees
            .Include(e => e.EducationRecords)
            .Where(e => documents.Contains(e.Document))
            .ToDictionaryAsync(e => e.Document);

        foreach (var row in importData)
        {
            // Resolve Department ID
            int? departmentId = null;
            if (!string.IsNullOrEmpty(row.Department) && deptMap.TryGetValue(row.Department, out int did))
            {
                departmentId = did;
            }

            Employee employee;
            if (!existingEmployees.TryGetValue(row.Document, out var existingEmp))
            {
                // Create NEW Employee
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
                
                if (departmentId.HasValue) employee.DepartmentId = departmentId.Value;

                _context.Employees.Add(employee);
                existingEmployees[row.Document] = employee;
            }
            else
            {
                employee = existingEmp;
                // Update Existing
                if (!string.IsNullOrEmpty(row.Email)) employee.Email = row.Email;
                if (!string.IsNullOrEmpty(row.Phone)) employee.Phone = row.Phone;
                if (!string.IsNullOrEmpty(row.Address)) employee.Address = row.Address;
                if (departmentId.HasValue) employee.DepartmentId = departmentId.Value;
            }

            // Update Employee fields
            if (decimal.TryParse(row.Salary, out var sal)) employee.Salary = sal;
            if (!string.IsNullOrEmpty(row.Position)) employee.Position = row.Position;
            if (!string.IsNullOrEmpty(row.Status)) employee.Status = row.Status;
            
            if (DateTime.TryParse(row.HireDate, out var hd)) 
                employee.HireDate = DateOnly.FromDateTime(hd);
            
            if (DateTime.TryParse(row.BirthDate, out var bd)) 
                employee.BirthDate = DateOnly.FromDateTime(bd);

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
