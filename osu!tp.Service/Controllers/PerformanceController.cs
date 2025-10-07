using Microsoft.AspNetCore.Mvc;
using osutp.Service.Models;
using osutp.TomPoints;

namespace osutp.Service.Controllers;

[Route("")]
public class PerformanceController : Controller
{
    [HttpPost]
    [Route("performance")]
    public IActionResult CalculatePerformance(
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