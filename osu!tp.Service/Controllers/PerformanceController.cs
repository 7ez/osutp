using Microsoft.AspNetCore.Mvc;
using osutp.Service.Models;
using osutp.TomPoints;

namespace osutp.Service.Controllers;

[Route("[controller]")]
public class PerformanceController : Controller
{
    [HttpPost]
    [Route("difficulty")]
    public IActionResult CalculateDifficulty(
        [FromBody] PerformanceCalculationRequest performanceCalculationRequest)
    {
        var performance = new TpPerformance(
            performanceCalculationRequest.Difficulty,
            performanceCalculationRequest.Score);

        var result = performance.ComputeTotalValue();

        if (result is null)
            return BadRequest();

        return Json(result);
    }
}