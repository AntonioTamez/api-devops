using Microsoft.AspNetCore.Mvc;

namespace DevOpsApi.Controllers;

/// <summary>
/// Controlador para obtener el estado y información del API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StatusController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StatusController> _logger;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    /// <summary>
    /// Constructor del StatusController
    /// </summary>
    public StatusController(
        IWebHostEnvironment environment, 
        IConfiguration configuration,
        ILogger<StatusController> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el estado actual del API
    /// </summary>
    /// <returns>Estado del API con información de ambiente y tiempo de actividad</returns>
    /// <response code="200">Retorna el estado actual del API</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        _logger.LogInformation("Status endpoint called - Environment: {Environment}", _environment.EnvironmentName);
        
        var uptime = DateTime.UtcNow - _startTime;

        return Ok(new
        {
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Environment = _environment.EnvironmentName,
            Version = "1.0.0",
            Uptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s"
        });
    }

    /// <summary>
    /// Obtiene información detallada del proyecto
    /// </summary>
    /// <returns>Información del API incluyendo características y enlaces</returns>
    /// <response code="200">Retorna información del proyecto</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetInfo()
    {
        _logger.LogInformation("Info endpoint called");
        
        return Ok(new
        {
            ApiName = "DevOps API",
            Description = "API REST automatizada con CI/CD, Terraform y despliegue en Azure Container Apps",
            Version = "1.0.0",
            Features = new[]
            {
                "Swagger/OpenAPI Documentation",
                "Health Checks",
                "CORS Configuration",
                "Structured Logging (Serilog)",
                "Docker Support",
                "Terraform IaC",
                "GitHub Actions CI/CD",
                "Azure Container Apps Deployment"
            },
            Repository = "https://github.com/your-org/api-devops",
            Documentation = $"{Request.Scheme}://{Request.Host}/swagger",
            HealthCheck = $"{Request.Scheme}://{Request.Host}/health",
            Environment = _environment.EnvironmentName,
            Timestamp = DateTime.UtcNow
        });
    }
}
