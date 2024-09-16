using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace MessengerApplication.Controllers;
[EnableCors("CorsPolicy")]
[ApiController]
[Route("/v1/api/")]
public class HealthCheckController : ControllerBase
{
   
   [HttpGet]
   public async Task<IActionResult> HealthCheck()
   {
      return Ok("Service is running");
   }
}