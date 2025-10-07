using osu.GameplayElements.Beatmaps;
using osu.GameplayElements.HitObjects;

namespace osutp.Service.Models;

public class DifficultyCalculationRequest
{
    public Mods Mods { get; set; } = Mods.None;
    public BeatmapBase Beatmap { get; set; } = null!;
    public List<HitObjectBase> HitObjects { get; set; } = null!;
}