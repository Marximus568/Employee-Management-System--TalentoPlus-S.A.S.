using Application.DTOs.Employee;
using Application.DTOs.PagedRequested;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api-v1/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IMapper _mapper;
    private readonly Application.Interfaces.PDF.IPdfService _pdfService;

    public EmployeesController(IEmployeeService employeeService, IMapper mapper, Application.Interfaces.PDF.IPdfService pdfService)
    {
        _employeeService = employeeService;
        _mapper = mapper;
        _pdfService = pdfService;
    }

    /// <summary>
    /// Gets the profile of the currently authenticated employee.
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value; // Or "email" if standard claim
        
        // Fallback for standard JwtRegisteredClaimNames.Email which is "email" or schema version
        if (string.IsNullOrEmpty(email))
            email = User.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var employee = await _employeeService.GetEmployeeByEmailAsync(email);
        if (employee == null) return NotFound("Employee profile not found for this account.");

        return Ok(employee);
    }

    /// <summary>
    /// Downloads the resume of the currently authenticated employee.
    /// </summary>
    [HttpGet("profile/resume")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadResume()
    {
         var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email || c.Type == "email" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
         
         if (string.IsNullOrEmpty(email)) return Unauthorized();

         var employee = await _employeeService.GetEmployeeByEmailAsync(email);
         if (employee == null) return NotFound("Employee profile not found.");

         try 
         {
            var pdfBytes = _pdfService.GenerateEmployeeResume(employee);
            var fileName = $"Resume_{employee.FirstName}_{employee.LastName}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
         }
         catch (Exception ex)
         {
             return StatusCode(500, $"Error generating PDF: {ex.Message}");
         }
    }

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto createEmployeeDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdEmployee = await _employeeService.CreateEmployeeAsync(createEmployeeDto);

        return CreatedAtAction(nameof(GetById), new { id = createdEmployee.Id }, createdEmployee);
    }

    /// <summary>
    /// Gets all employees with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedRequestDto<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        var pagedResult = await _employeeService.GetEmployeesPagedAsync(page, pageSize);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Gets a single employee by its ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);

        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    /// <summary>
    /// Updates an employee.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedEmployee = await _employeeService.UpdateEmployeeAsync(id, updateDto);

        if (updatedEmployee == null)
            return NotFound();

        return Ok(updatedEmployee);
    }

    /// <summary>
    /// Deletes an employee by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _employeeService.DeleteEmployeeAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}
