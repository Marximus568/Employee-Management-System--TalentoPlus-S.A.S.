using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Front_end.Web.Pages.Dashboard.Excel;

[Authorize]
public class IndexModel : PageModel
{
    private readonly Application.Interfaces.Excel.IExcelService _excelService;

    public IndexModel(Application.Interfaces.Excel.IExcelService excelService)
    {
        _excelService = excelService;
    }

    [BindProperty]
    public IFormFile Upload { get; set; }

    public string Message { get; set; }
    public bool IsSuccess { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostImportAsync()
    {
        if (Upload == null || Upload.Length == 0)
        {
            Message = "Please select a file to upload.";
            IsSuccess = false;
            return Page();
        }

        if (!Path.GetExtension(Upload.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            Message = "Invalid file format. Please upload an .xlsx file.";
            IsSuccess = false;
            return Page();
        }

        try
        {
            await _excelService.ImportEmployeesAsync(Upload.OpenReadStream());
            Message = "Employees imported successfully!";
            IsSuccess = true;
        }
        catch (Exception ex)
        {
            Message = $"Error importing file: {ex.Message}";
            IsSuccess = false;
        }

        return Page();
    }
}
