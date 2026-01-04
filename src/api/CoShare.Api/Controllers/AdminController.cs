using Microsoft.AspNetCore.Mvc;

namespace CoShare.Api.Controllers;

/// <summary>
/// Mock Admin controller for demo purposes.
/// Provides company list for registration dropdown.
/// </summary>
[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    /// <summary>
    /// Get list of companies for dropdown.
    /// Mock endpoint for demo.
    /// </summary>
    [HttpGet("companies")]
    public IActionResult GetCompanies()
    {
        var correlationId = HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
        
        Response.Headers["X-Correlation-Id"] = correlationId;

        // Mock company data for demo
        var companies = new
        {
            items = new[]
            {
                new { id = 501, companyCode = "CTY001", companyName = "Công ty Điện tử KCN X", zone = "KCN X", status = "ACTIVE" },
                new { id = 502, companyCode = "CTY002", companyName = "Công ty May mặc ABC", zone = "KCN Y", status = "ACTIVE" },
                new { id = 503, companyCode = "CTY003", companyName = "Công ty Thực phẩm XYZ", zone = "KCN Z", status = "ACTIVE" },
                new { id = 504, companyCode = "CTY004", companyName = "Công ty Linh kiện 123", zone = "KCN X", status = "ACTIVE" },
            },
            page = 1,
            pageSize = 20,
            totalItems = 4,
            totalPages = 1
        };

        return Ok(companies);
    }
}
