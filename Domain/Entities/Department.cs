namespace Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Reverse Navigation
    public ICollection<Employee> Employees { get; set; }
}