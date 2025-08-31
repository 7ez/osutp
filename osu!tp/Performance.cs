using System;

namespace osutp.TomPoints
{
    public class TpPerformance
    {
        public TpDifficultyCalculation Difficulty;
        public TpScore Score;

        public TpPerformance(TpDifficultyCalculation difficulty, TpScore score)
        {
            Difficulty = difficulty;
            Score = score;
        }

        public TpPerformanceResult ComputeTotalValue()
        {
            if (Score.IsRelaxing() || Score.IsAutoplay())
                return new TpPerformanceResult();
            
            // Custom multipliers for NoFail and SpunOut
            double multiplier = 1.1f;

            if (Score.Mods.HasFlag(Mods.NoFail))
                multiplier *= 0.9f;

            if (Score.Mods.HasFlag(Mods.SpunOut))
                multiplier *= 0.95f;

            double aim = ComputeAimValue();
            double speed = ComputeSpeedValue();
            double acc = ComputeAccValue();
            double attributes = 
                Math.Pow(aim, 1.1f) +
                Math.Pow(speed, 1.1f) +
                Math.Pow(acc, 1.1f);

            double total = Math.Pow(attributes, 1.0f / 1.1f) * multiplier;

            return new TpPerformanceResult
            {
                Total = total,
                Aim = aim,
                Speed = speed,
                Acc = acc,
            };
        }

        public double ComputeAimValue()
        {
            double aimValue = Math.Pow(5.0f * Math.Max(1.0f, Difficulty.AimStars / 0.036f) - 4.0f, 3.0f) / 100000.0f;

            // Longer maps are worth more
            aimValue *= 1 + 0.1f * Math.Min(1.0f, Score.TotalHits() / 1500.0f);

            // Penalize misses exponentially
            // This mainly fixes TAG4 maps and the likes until a per-hitobject solution is available
            aimValue *= Math.Pow(0.97f, Score.AmountMiss);

            // Combo scaling
            if (Difficulty.MaxCombo > 0)
            {
                aimValue *= Math.Min(1.0f, Math.Pow(Score.MaxCombo, 0.8f) / Math.Pow(Difficulty.MaxCombo, 0.8f));
            }

            float approachRateFactor = 1.0f;

            if (Difficulty.ApproachRate > 10.0f)
            {
                approachRateFactor += 0.30f * (Difficulty.ApproachRate - 10.0f);
            }
            else if (Difficulty.ApproachRate < 8.0f)
            {
                // Hidden is worth more with lower AR
                if (Score.Mods.HasFlag(Mods.Hidden))
                {
                    approachRateFactor += 0.02f * (8.0f - Difficulty.ApproachRate);
                }
                else
                {
                    approachRateFactor += 0.01f * (8.0f - Difficulty.ApproachRate);
                }
            }

            aimValue *= approachRateFactor;

            // Hidden Bonus
            if (Score.Mods.HasFlag(Mods.Hidden))
            {
                aimValue *= 1.18f;
            }

            // Flashlight Bonus
            if (Score.Mods.HasFlag(Mods.Flashlight))
            {
                aimValue *= 1.36f;
            }

            // Scale aim value with accuracy, slightly
            aimValue *= 0.5f + Score.Accuracy() / 2.0f;

            // It is important to also consider overall difficulty when doing that
            aimValue *= 0.98f + (Math.Pow(Difficulty.OverallDifficulty, 2) / 2500);

            return aimValue;
        }

        public double ComputeSpeedValue()
        {
            double speedValue = Math.Pow(5.0f * Math.Max(1.0f, Difficulty.SpeedStars / 0.036f) - 4.0f, 3.0f) / 100000.0f;

            // Longer maps are worth more
            speedValue *= 1 + 0.1f * Math.Min(1.0f, Score.TotalHits() / 1500.0f);

            // Penalize misses exponentially
            // This mainly fixes TAG4 maps and the likes until a per-hitobject solution is available
            speedValue *= Math.Pow(0.97f, Score.AmountMiss);

            // Combo scaling
            if (Difficulty.MaxCombo > 0)
            {
                speedValue *= Math.Min(1.0f, Math.Pow(Score.MaxCombo, 0.8f) / Math.Pow(Difficulty.MaxCombo, 0.8f));
            }

            // Scale aim value with accuracy, slightly
            speedValue *= 0.5f + Score.Accuracy() / 2.0f;

            // It is important to also consider overall difficulty when doing that
            speedValue *= 0.98f + (Math.Pow(Difficulty.OverallDifficulty, 2) / 2500);

            return speedValue;
        }

        public double ComputeAccValue()
        {
            // This percentage only considers HitCircles of any value
            // In this part of the calculation we focus on hitting the timing hit window
            double betterAccuracyPercentage = 0;

            if (Difficulty.AmountNormal > 0)
            {
                betterAccuracyPercentage =
                    (float)((Score.Amount300 - (Score.TotalHits() - Difficulty.AmountNormal)) * 6 + Score.Amount100 * 2 + Score.Amount50)
                    / (Difficulty.AmountNormal * 6);
            }

            // It is possible to reach a negative accuracy with this formula. Cap it at zero - zero points
            if (betterAccuracyPercentage < 0)
            {
                betterAccuracyPercentage = 0;
            }

            // Lots of arbitrary values from testing.
            // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            double accValue =
                Math.Pow(
                    Math.Pow(1.3f, Difficulty.OverallDifficulty) * Math.Pow(betterAccuracyPercentage, 15) / 2,
                    1.6f
                ) *
                8.3f;

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer
            accValue *= Math.Min(1.15f, Math.Pow(Difficulty.AmountNormal / 1000.0f, 0.3f));

            // Hidden Bonus
            if (Score.Mods.HasFlag(Mods.Hidden))
            {
                accValue *= 1.02f;
            }

            // Flashlight Bonus
            if (Score.Mods.HasFlag(Mods.Flashlight))
            {
                accValue *= 1.02f;
            }

            return accValue;
        }
    }
}
