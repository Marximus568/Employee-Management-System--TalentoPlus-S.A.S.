using Domain.Entities;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context;

/// <summary>
/// Database context for application data (non-Identity)
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Person> Persons { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<EmployeeEducation> EmployeeEducations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Person
        modelBuilder.Entity<Person>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Person>()
            .HasOne(p => p.Employee)
            .WithOne(e => e.Person)
            .HasForeignKey<Employee>(e => e.PersonId);

        // Employee
        modelBuilder.Entity<Employee>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId);

        // EmployeeEducation
        modelBuilder.Entity<EmployeeEducation>()
            .HasKey(ed => ed.Id);

        modelBuilder.Entity<EmployeeEducation>()
            .HasOne(ed => ed.Employee)
            .WithMany(e => e.EducationRecords)
            .HasForeignKey(ed => ed.EmployeeId);
    }
}