using Application.DTOs.Employee;
using Application.DTOs.PagedRequested;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.EmployeeService;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ApplicationDbContext context, IMapper mapper, ILogger<EmployeeService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new employee in the system.
    /// </summary>
    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto)
    {
        _logger.LogInformation("Creating new employee with document {Document}", createEmployeeDto.Document);

        var employee = _mapper.Map<Employee>(createEmployeeDto);

        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();

        // Load related Department if needed
        await _context.Entry(employee).Reference(e => e.Department).LoadAsync();

        return _mapper.Map<EmployeeDto>(employee);
    }

    /// <summary>
    /// Gets employees in a paginated way.
    /// </summary>
    public async Task<EmployeeDto?> GetEmployeeByEmailAsync(string email)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Email == email);

        return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
    }

    public async Task<PagedRequestDto<EmployeeDto>> GetEmployeesPagedAsync(int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = _context.Employees
            .Include(e => e.Department)
            .AsNoTracking();

        var totalItems = await query.CountAsync();

        var employees = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);

        return new PagedRequestDto<EmployeeDto>
        {
            Items = employeeDtos,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    /// <summary>
    /// Gets a single employee by ID.
    /// </summary>
    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
    }

    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    public async Task<EmployeeDto?> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateEmployeeDto)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return null;

        // Map updated fields
        _mapper.Map(updateEmployeeDto, employee);

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();

        return _mapper.Map<EmployeeDto>(employee);
    }

    /// <summary>
    /// Deletes an employee by ID.
    /// </summary>
    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
            return false;

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return true;
    }
}
