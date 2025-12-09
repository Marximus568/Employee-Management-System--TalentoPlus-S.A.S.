namespace Domain.Entities;

public class EmployeeEducation
{
    public int Id { get; set; }

    // Foreign Key to Employee
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }

    public string EducationLevel { get; set; }
    public string ProfessionalProfile { get; set; }
}