using Domain.Entities;

namespace Domain.Models;

public class Person
{
    // Primary Key
    public int Id { get; set; }

    // Core Identity Information
    public string Document { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public DateTime BirthDate { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }

    // Navigation: One Person → One Employee
    public Employee Employee { get; set; }
}