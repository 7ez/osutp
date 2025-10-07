using Microsoft.AspNetCore.Mvc;
using osutp.Service.Models;
using osutp.TomPoints;

namespace osutp.Service.Controllers;

[Route("")]
public class DifficultyController : Controller
{
    [HttpPost]
    [Route("difficulty")]
    public IActionResult CalculateDifficulty(
        [FromBody] DifficultyCalculationRequest difficultyCalculationRequest)
    {
        var difficulty = new TpDifficulty();
        var result = difficulty.Process(
            difficultyCalculationRequest.Beatmap,
            difficultyCalculationRequest.HitObjects,
            difficultyCalculationRequest.Mods);

        if (result is null)
            return BadRequest();

        return Json(result);
    }
}