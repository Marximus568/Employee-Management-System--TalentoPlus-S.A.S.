
using Application.DTOs.Employee;
using Application.DTOs.PagedRequested;

namespace Application.Interfaces;

/// <summary>
/// Application-level contract for employee business logic.
/// This service abstracts all operations related to employees,
/// enforcing clean architecture principles and DTO separation.
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Creates a new employee inside the system.
    /// </summary>
    /// <param name="createEmployeeDto">Employee creation payload.</param>
    /// <returns>Newly created employee represented as a DTO.</returns>
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto);

    /// <summary>
    /// Retrieves paged employee records using efficient projection
    /// and optional filtering/searching logic.
    /// </summary>
    /// <param name="page">Page number starting from 1.</param>
    /// <param name="pageSize">Size of each page.</param>
    /// <returns>A paginated list of employees.</returns>
    Task<PagedRequestDto<EmployeeDto>> GetEmployeesPagedAsync(int page, int pageSize);

    /// <summary>
    /// Gets a single employee by ID.
    /// Returns null when the employee does not exist.
    /// </summary>
    /// <param name="id">Employee identifier.</param>
    /// <returns>Employee DTO or null.</returns>
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    
    /// <summary>
    /// Gets a single employee by Email.
    /// Returns null when the employee does not exist.
    /// </summary>
    /// <param name="email">Employee email.</param>
    /// <returns>Employee DTO or null.</returns>
    Task<EmployeeDto?> GetEmployeeByEmailAsync(string email);



    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    /// <param name="id">Employee identifier to update.</param>
    /// <param name="updateEmployeeDto">Updated employee payload.</param>
    /// <returns>The updated employee DTO or null if not found.</returns>
    Task<EmployeeDto?> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateEmployeeDto);

    /// <summary>
    /// Deletes an employee by its ID.
    /// </summary>
    /// <param name="id">Employee identifier to delete.</param>
    /// <returns>True if the employee was deleted, false if not found.</returns>
    Task<bool> DeleteEmployeeAsync(int id);
}