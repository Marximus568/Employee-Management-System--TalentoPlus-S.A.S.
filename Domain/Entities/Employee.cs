using Domain.Models;

namespace Domain.Entities;

public class Employee
{
    // Primary Key
    public int Id { get; set; }

    // Foreign Key Relationship with Person
    public int PersonId { get; set; }
    public Person Person { get; set; }

    // Job Information
    public string Position { get; set; }
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
    public string Status { get; set; }

    // Foreign Key to Department
    public int DepartmentId { get; set; }
    public Department Department { get; set; }

    // One Employee -> Many Education Records
    public ICollection<EmployeeEducation> EducationRecords { get; set; }
}