using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Health check controller for service monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint for service monitoring
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet]
    public ActionResult Health()
    {
        return Ok(new { Status = "Healthy", Service = "TaskManagementAPI", Timestamp = DateTime.UtcNow });
    }
}