using Application.DTOs.Employee;

namespace Application.Interfaces.PDF;

public interface IPdfService
{
    byte[] GenerateEmployeeResume(EmployeeDto employee);
}
