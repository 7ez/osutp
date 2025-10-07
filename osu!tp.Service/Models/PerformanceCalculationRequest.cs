using osutp.TomPoints;

namespace osutp.Service.Models;

public class PerformanceCalculationRequest
{
    public TpDifficultyCalculation Difficulty { get; set; } = null!;
    public TpScore Score { get; set; } = null!;
}