using Domain.Entities;

namespace Domain.Models;

public abstract class Person : BaseEntity
{
    // Primary Key
    public int Id { get; set; }

    // Core Identity Information
    public string Document { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public DateOnly BirthDate { get; set; }
    public string Address { get; set; }  = string.Empty;  
    public string Phone { get; set; }
    public string Email { get; set; }
}