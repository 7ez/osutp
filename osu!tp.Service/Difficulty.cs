using osu.GameplayElements.Beatmaps;
using osu.GameplayElements.HitObjects;
using osutp.TomPoints;

namespace osutp.Service
{
    public class DifficultyCalculationRequest
    {
        public BeatmapBase? Beatmap;
        public List<HitObjectBase>? HitObjects;
    }

    public class Difficulty
    {
        public static TpDifficultyCalculation BeatmapDifficultyCalculation(HttpContext httpContext)
        {
            return null;
        }
    }
}
