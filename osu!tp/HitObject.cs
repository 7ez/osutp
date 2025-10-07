using System;
using System.Numerics;
using osu.GameplayElements.HitObjects;

namespace osutp.TomPoints;

internal class TpHitObject
{
    // Almost the normed diameter of a circle (104 osu pixel). That is -after- position transforming.
    private const double AlmostDiameter = 90; 

    // Pseudo threshold values to distinguish between "singles" and "streams". Of course the border can not be defined clearly, therefore the algorithm
    // has a smooth transition between those values. They also are based on tweaking and general feedback.
    private const double StreamSpacingTreshold = 110;
    private const double SingleSpacingTreshold = 125;

    // In milliseconds. The smaller the value, the more accurate sliders are approximated. 0 leads to an infinite loop, so use something bigger.
    private const int LazySliderStepLength = 1;

    // Factor by how much speed / aim strain decays per second. Those values are results of tweaking a lot and taking into account general feedback.
    // Opinionated observation: Speed is easier to maintain than accurate jumps.
    public static readonly double[] DecayBase = { 0.3, 0.15 }; 

    // Scaling values for weightings to keep aim and speed difficulty in balance. Found from testing a very large map pool (containing all ranked maps) and keeping the
    // average values the same.
    private static readonly double[] SpacingWeightScaling = { 1400, 26.25 };

    public readonly HitObjectBase BaseHitObject;
    public readonly int Combo;
    private readonly float _lazySliderLengthFirst;
    private readonly float _lazySliderLengthSubsequent;
    private readonly Vector2 _normalizedEndPosition;

    private readonly Vector2 _normalizedStartPosition;
    public readonly double[] Strains = { 1, 1 };

    public TpHitObject(HitObjectBase baseHitObject, float circleRadius)
    {
        BaseHitObject = baseHitObject;

        // We will scale everything by this factor, so we can assume a uniform CircleSize among beatmaps.
        var scalingFactor = 52.0f / circleRadius;
        _normalizedStartPosition = baseHitObject.Position * scalingFactor;

        // Calculate approximation of lazy movement on the slider
        if ((baseHitObject.Type & HitObjectType.Slider) > 0)
        {
            var sliderFollowCircleRadius =
                circleRadius *
                3; // Not sure if this is correct, but here we do not need 100% exact values. This comes pretty darn close in my tests.

            var segmentLength = baseHitObject.Length / baseHitObject.SegmentCount;
            var segmentEndTime = baseHitObject.StartTime + segmentLength;

            // For simplifying this step we use actual osu! coordinates and simply scale the length, that we obtain by the ScalingFactor later
            var cursorPos = baseHitObject.Position;

            // Actual computation of the first lazy curve
            for (var time = baseHitObject.StartTime + LazySliderStepLength;
                 time < segmentEndTime;
                 time += LazySliderStepLength)
            {
                var difference = baseHitObject.PositionAtTime(time) - cursorPos;
                var distance = difference.Length();

                // Did we move away too far?
                if (distance > sliderFollowCircleRadius)
                {
                    // Yep, we need to move the cursor
                    difference =
                        Vector2.Normalize(
                            difference); // Obtain the direction of difference. We do no longer need the actual difference
                    distance -= sliderFollowCircleRadius;
                    cursorPos +=
                        difference * distance; // We move the cursor just as far as needed to stay in the follow circle
                    _lazySliderLengthFirst += distance;
                }
            }

            _lazySliderLengthFirst *= scalingFactor;

            // If we have an odd amount of repetitions the current position will be the end of the slider. Note that this will -always- be triggered if
            // BaseHitObject.SegmentCount <= 1, because BaseHitObject.SegmentCount can not be smaller than 1. Therefore NormalizedEndPosition will always be initialized
            if (baseHitObject.SegmentCount % 2 == 1) _normalizedEndPosition = cursorPos * scalingFactor;

            // If we have more than one segment, then we also need to compute the length ob subsequent lazy curves. They are different from the first one, since the first
            // one starts right at the beginning of the slider.
            if (baseHitObject.SegmentCount > 1)
            {
                // Use the next segment
                segmentEndTime += segmentLength;

                for (var time = segmentEndTime - segmentLength + LazySliderStepLength;
                     time < segmentEndTime;
                     time += LazySliderStepLength)
                {
                    var difference = baseHitObject.PositionAtTime(time) - cursorPos;
                    var distance = difference.Length();

                    // Did we move away too far?
                    if (distance > sliderFollowCircleRadius)
                    {
                        // Yep, we need to move the cursor
                        difference =
                            Vector2.Normalize(
                                difference); // Obtain the direction of difference. We do no longer need the actual difference
                        distance -= sliderFollowCircleRadius;
                        cursorPos +=
                            difference *
                            distance; // We move the cursor just as far as needed to stay in the follow circle
                        _lazySliderLengthSubsequent += distance;
                    }
                }

                _lazySliderLengthSubsequent *= scalingFactor;
                // If we have an even amount of repetitions the current position will be the end of the slider
                if (baseHitObject.SegmentCount % 2 == 1) _normalizedEndPosition = cursorPos * scalingFactor;
            }

            // TODO: Calculate the combo for this slider properly
            //       This is just an approximation for now
            double sliderTravel =
                _lazySliderLengthFirst +
                _lazySliderLengthSubsequent * (baseHitObject.SegmentCount - 1);

            Combo = (int)(sliderTravel / AlmostDiameter);
        }
        // We have a normal HitCircle or a spinner
        else
        {
            _normalizedEndPosition = baseHitObject.EndPosition * scalingFactor;
            Combo = 1;
        }
    }

    public void CalculateStrains(TpHitObject previousHitObject, double timeRate)
    {
        CalculateSpecificStrain(previousHitObject, TpDifficulty.DifficultyType.Speed, timeRate);
        CalculateSpecificStrain(previousHitObject, TpDifficulty.DifficultyType.Aim, timeRate);
    }

    // Caution: The subjective values are strong with this one
    private static double SpacingWeight(double distance, TpDifficulty.DifficultyType type)
    {
        switch (type)
        {
            case TpDifficulty.DifficultyType.Speed:
            {
                var weight = 0d;

                if (distance > SingleSpacingTreshold)
                    weight = 2.5;
                else if (distance > StreamSpacingTreshold)
                    weight = 1.6 + 0.9 * (distance - StreamSpacingTreshold) /
                        (SingleSpacingTreshold - StreamSpacingTreshold);
                else if (distance > AlmostDiameter)
                    weight = 1.2 + 0.4 * (distance - AlmostDiameter) / (StreamSpacingTreshold - AlmostDiameter);
                else if (distance > AlmostDiameter / 2)
                    weight = 0.95 + 0.25 * (distance - AlmostDiameter / 2) / (AlmostDiameter / 2);
                else
                    weight = 0.95;

                return weight;
            }

            case TpDifficulty.DifficultyType.Aim:
                return Math.Pow(distance, 0.99);

            default:
                // Should never happen. 
                return 0;
        }
    }

    private void CalculateSpecificStrain(TpHitObject previousHitObject, TpDifficulty.DifficultyType type,
        double timeRate)
    {
        var timeElapsed = (BaseHitObject.StartTime - previousHitObject.BaseHitObject.StartTime) / timeRate;
        var decay = Math.Pow(DecayBase[(int)type], timeElapsed / 1000);
        double addition = 1;

        if ((BaseHitObject.Type & HitObjectType.Spinner) > 0)
        {
            // Do nothing for spinners
        }
        else if ((BaseHitObject.Type & HitObjectType.Slider) > 0)
        {
            addition = type switch
            {
                TpDifficulty.DifficultyType.Speed =>
                    // For speed strain we treat the whole slider as a single spacing entity, since "Speed" is about how hard it is to click buttons fast.
                    // The spacing weight exists to differentiate between being able to easily alternate or having to single.
                    SpacingWeight(
                        previousHitObject._lazySliderLengthFirst +
                        previousHitObject._lazySliderLengthSubsequent *
                        (previousHitObject.BaseHitObject.SegmentCount - 1) + DistanceTo(previousHitObject), type) *
                    SpacingWeightScaling[(int)type],
                TpDifficulty.DifficultyType.Aim =>
                    // For Aim strain we treat each slider segment and the jump after the end of the slider as separate jumps, since movement-wise there is no difference
                    // to multiple jumps.
                    (SpacingWeight(previousHitObject._lazySliderLengthFirst, type) +
                     SpacingWeight(previousHitObject._lazySliderLengthSubsequent, type) *
                     (previousHitObject.BaseHitObject.SegmentCount - 1) +
                     SpacingWeight(DistanceTo(previousHitObject), type)) * SpacingWeightScaling[(int)type],
                _ => addition
            };
        }
        else if ((BaseHitObject.Type & HitObjectType.Normal) > 0)
        {
            addition = SpacingWeight(DistanceTo(previousHitObject), type) * SpacingWeightScaling[(int)type];
        }

        // Scale addition by the time, that elapsed. Filter out HitObjects that are too close to be played anyway to avoid crazy values by division through close to zero.
        // You will never find maps that require this amongst ranked maps.
        addition /= Math.Max(timeElapsed, 50);

        Strains[(int)type] = previousHitObject.Strains[(int)type] * decay + addition;
    }

    private double DistanceTo(TpHitObject other)
    {
        // Scale the distance by circle size.
        return (_normalizedStartPosition - other._normalizedEndPosition).Length();
    }
}