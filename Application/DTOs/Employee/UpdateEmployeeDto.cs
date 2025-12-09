namespace Application.DTOs.Employee;

public class UpdateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty; 
    public string LastName { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";  
        
    // FK
    public int DepartmentId { get; set; }
}