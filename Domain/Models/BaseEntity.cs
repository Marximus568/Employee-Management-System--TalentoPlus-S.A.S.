namespace Domain.Models;

public abstract class BaseEntity
{
    public int Id { get; set; }            // PK
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}