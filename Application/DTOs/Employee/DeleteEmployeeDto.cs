namespace Application.DTOs.Employee;

public class DeleteEmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty; 
    public string Document { get; set; } = string.Empty;
}