using System;
using System.Collections.Generic;
using System.Linq;
using osu.GameplayElements.Beatmaps;
using osu.GameplayElements.HitObjects;

namespace osutp.TomPoints;

/// <summary>
///     osu!tp's difficulty calculator ported to the osu! sdk as far as so far possible.
/// </summary>
public class TpDifficulty
{
    // Those values are used as array indices. Be careful when changing them!
    public enum DifficultyType
    {
        Speed = 0,
        Aim
    }

    private const double StarScalingFactor = 0.045;
    private const double ExtremeScalingFactor = 0.5;
    private const float PlayfieldWidth = 512;

    // In milliseconds. For difficulty calculation we will only look at the highest strain value in each time interval of size STRAIN_STEP.
    // This is to eliminate higher influence of stream over aim by simply having more HitObjects with high strain.
    // The higher this value, the less strains there will be, indirectly giving long beatmaps an advantage.
    private const double StrainStep = 400;

    // The weighting of each strain value decays to 0.9 * it's previous value
    private const double DecayWeight = 0.9;
    private float _timeRate = 1.0f;

    private static double MapDifficultyRange(double difficulty, double min, double mid, double max)
    {
        if (difficulty > 5)
            return mid + (max - mid) * (difficulty - 5) / 5;
        if (difficulty < 5)
            return mid - (mid - min) * (5 - difficulty) / 5;
        return mid;
    }

    private void AdjustDifficulty(BeatmapBase beatmap, Mods mods)
    {
        if (mods.HasFlag(Mods.HardRock))
        {
            beatmap.DifficultyOverall = Math.Min(beatmap.DifficultyOverall * 1.4f, 10);
            beatmap.DifficultyCircleSize = Math.Min(beatmap.DifficultyCircleSize * 1.3f, 10);
            beatmap.DifficultyHpDrainRate = Math.Min(beatmap.DifficultyHpDrainRate * 1.4f, 10);
            beatmap.DifficultyApproachRate = Math.Min(beatmap.DifficultyApproachRate * 1.4f, 10);
        }

        if (mods.HasFlag(Mods.Easy))
        {
            beatmap.DifficultyOverall = Math.Max(beatmap.DifficultyOverall / 2, 0);
            beatmap.DifficultyCircleSize = Math.Max(beatmap.DifficultyCircleSize / 2, 0);
            beatmap.DifficultyHpDrainRate = Math.Max(beatmap.DifficultyHpDrainRate / 2, 0);
            beatmap.DifficultyApproachRate = Math.Max(beatmap.DifficultyApproachRate / 2, 0);
        }

        if (mods.HasFlag(Mods.DoubleTime) || mods.HasFlag(Mods.Nightcore))
        {
            _timeRate = 1.5f;
            RecalculateBeatmapDifficulty(beatmap);
        }

        if (mods.HasFlag(Mods.HalfTime))
        {
            _timeRate = 0.75f;
            RecalculateBeatmapDifficulty(beatmap);
        }
    }

    private void RecalculateBeatmapDifficulty(BeatmapBase beatmap)
    {
        var preEmpt = MapDifficultyRange(beatmap.DifficultyApproachRate, 1800, 1200, 450) / _timeRate;
        var hitWindow300 = MapDifficultyRange(beatmap.DifficultyOverall, 80, 50, 20) / _timeRate;
        beatmap.DifficultyOverall = (float)(-(hitWindow300 - 80.0) / 6.0);
        beatmap.DifficultyApproachRate = (float)(preEmpt > 1200 ? (1800 - preEmpt) / 120 : (1200 - preEmpt) / 150 + 5);
    }

    public TpDifficultyCalculation? Process(BeatmapBase beatmap, List<HitObjectBase> hitObjects, Mods mods)
    {
        // Adjust beatmap attributes, based on relevant mods
        AdjustDifficulty(beatmap, mods);

        // Fill our custom tpHitObject class, that carries additional information
        var tpHitObjects = new List<TpHitObject>(hitObjects.Count);
        var circleRadius = PlayfieldWidth / 16.0f * (1.0f - 0.7f * (beatmap.DifficultyCircleSize - 5.0f) / 5.0f);

        tpHitObjects.AddRange(hitObjects.Select(hitObject => new TpHitObject(hitObject, circleRadius)));

        // Sort tpHitObjects by StartTime of the HitObjects - just to make sure. Not using CompareTo, since it results in a crash (HitObjectBase inherits MarshalByRefObject)
        tpHitObjects.Sort((a, b) => a.BaseHitObject.StartTime - b.BaseHitObject.StartTime);

        if (!CalculateStrainValues(tpHitObjects, _timeRate))
            return null;

        var speedDifficulty = CalculateDifficulty(tpHitObjects, DifficultyType.Speed);
        var aimDifficulty = CalculateDifficulty(tpHitObjects, DifficultyType.Aim);

        // OverallDifficulty is not considered in this algorithm and neither is HpDrainRate. That means, that in this form the algorithm determines how hard it physically is
        // to play the map, assuming, that too much of an error will not lead to a death.
        // It might be desirable to include OverallDifficulty into map difficulty, but in my personal opinion it belongs more to the weighting of the actual peformance
        // and is superfluous in the beatmap difficulty rating.
        // If it were to be considered, then I would look at the hit window of normal HitCircles only, since Sliders and Spinners are (almost) "free" 300s and take map length
        // into account as well.

        // The difficulty can be scaled by any desired metric.
        // In osu!tp it gets squared to account for the rapid increase in difficulty as the limit of a human is approached. (Of course it also gets scaled afterwards.)
        // It would not be suitable for a star rating, therefore:

        // The following is a proposal to forge a star rating from 0 to 5. It consists of taking the square root of the difficulty, since by simply scaling the easier
        // 5-star maps would end up with one star.
        var speedStars = Math.Sqrt(speedDifficulty) * StarScalingFactor;
        var aimStars = Math.Sqrt(aimDifficulty) * StarScalingFactor;

        // Again, from own observations and from the general opinion of the community a map with high speed and low aim (or vice versa) difficulty is harder,
        // than a map with mediocre difficulty in both. Therefore we can not just add both difficulties together, but will introduce a scaling that favors extremes.
        var starRating = speedStars + aimStars + Math.Abs(speedStars - aimStars) * ExtremeScalingFactor;

        // Another approach to this would be taking Speed and Aim separately to a chosen power, which again would be equivalent. This would be more convenient if
        // the hit window size is to be considered as well.

        // Note: The star rating is tuned extremely tight! Airman (/b/104229) and Freedom Dive (/b/126645), two of the hardest ranked maps, both score ~4.66 stars.
        // Expect the easier kind of maps that officially get 5 stars to obtain around 2 by this metric. The tutorial still scores about half a star.
        // Tune by yourself as you please. ;)
        return new TpDifficultyCalculation
        {
            AmountNormal = tpHitObjects.FindAll(x => x.BaseHitObject.Type.HasFlag(HitObjectType.Normal)).Count,
            AmountSliders = tpHitObjects.FindAll(x => x.BaseHitObject.Type.HasFlag(HitObjectType.Slider)).Count,
            AmountSpinners = tpHitObjects.FindAll(x => x.BaseHitObject.Type.HasFlag(HitObjectType.Spinner)).Count,
            MaxCombo = tpHitObjects.Sum(x => x.Combo),
            SpeedDifficulty = speedDifficulty,
            AimDifficulty = aimDifficulty,
            StarRating = starRating,
            SpeedStars = speedStars,
            AimStars = aimStars,
            OverallDifficulty = beatmap.DifficultyOverall,
            ApproachRate = beatmap.DifficultyApproachRate,
            HpDrainRate = beatmap.DifficultyHpDrainRate,
            CircleSize = beatmap.DifficultyCircleSize,
            SliderTickRate = beatmap.DifficultySliderTickRate,
            SliderMultiplier = beatmap.DifficultySliderMultiplier
        };
    }

    // Exceptions would be nicer to handle errors, but for this small project it shall be ignored.
    private static bool CalculateStrainValues(List<TpHitObject> tpHitObjects, double timeRate)
    {
        // Traverse hitObjects in pairs to calculate the strain value of NextHitObject from the strain value of CurrentHitObject and environment.
        var hitObjectsEnumerator = tpHitObjects.GetEnumerator();

        if (!hitObjectsEnumerator.MoveNext())
            // Can not compute difficulty of empty beatmap
            return false;

        var currentHitObject = hitObjectsEnumerator.Current;

        // First hitObject starts at strain 1. 1 is the default for strain values, so we don't need to set it here. See tpHitObject.
        while (hitObjectsEnumerator.MoveNext())
        {
            var nextHitObject = hitObjectsEnumerator.Current;
            nextHitObject.CalculateStrains(currentHitObject, timeRate);
            currentHitObject = nextHitObject;
        }

        return true;
    }

    private double CalculateDifficulty(List<TpHitObject> tpHitObjects, DifficultyType type)
    {
        var actualStrainStep = StrainStep * _timeRate;

        // Find the highest strain value within each strain step
        var highestStrains = new List<double>();
        var intervalEndTime = actualStrainStep;
        double maximumStrain = 0; // We need to keep track of the maximum strain in the current interval

        TpHitObject? previousHitObject = null;
        foreach (var hitObject in tpHitObjects)
        {
            // While we are beyond the current interval push the currently available maximum to our strain list
            while (hitObject.BaseHitObject.StartTime > intervalEndTime)
            {
                highestStrains.Add(maximumStrain);

                // The maximum strain of the next interval is not zero by default! We need to take the last hitObject we encountered, take its strain and apply the decay
                // until the beginning of the next interval.
                if (previousHitObject == null)
                {
                    maximumStrain = 0;
                }
                else
                {
                    var decay = Math.Pow(TpHitObject.DecayBase[(int)type],
                        (intervalEndTime - previousHitObject.BaseHitObject.StartTime) / 1000);
                    maximumStrain = previousHitObject.Strains[(int)type] * decay;
                }

                // Go to the next time interval
                intervalEndTime += actualStrainStep;
            }

            // Obtain maximum strain
            if (hitObject.Strains[(int)type] > maximumStrain) maximumStrain = hitObject.Strains[(int)type];

            previousHitObject = hitObject;
        }

        // Build the weighted sum over the highest strains for each interval
        var difficulty = 0d;
        var weight = 1d;

        // Sort from highest to lowest strain.
        highestStrains.Sort((a, b) => b.CompareTo(a));

        foreach (var strain in highestStrains)
        {
            difficulty += weight * strain;
            weight *= DecayWeight;
        }

        return difficulty;
    }
}