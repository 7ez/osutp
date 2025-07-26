using System;

namespace osutp.TomPoints
{
    public class TpScore
    {
        public string BeatmapFilename;
        public string BeatmapChecksum;
        public int TotalScore;
        public int MaxCombo;
        public int Amount300;
        public int Amount100;
        public int Amount50;
        public int AmountMiss;
        public int AmountGeki;
        public int AmountKatu;
        public Mods Mods;

        public TpScore(string beatmapFilename, string beatmapChecksum, int totalScore, int maxCombo, int amount300, int amount100, int amount50, int amountMiss, int amountGeki, int amountKatu, Mods mods)
        {
            BeatmapFilename = beatmapFilename;
            BeatmapChecksum = beatmapChecksum;
            TotalScore = totalScore;
            MaxCombo = maxCombo;
            Amount300 = amount300;
            Amount100 = amount100;
            Amount50 = amount50;
            AmountMiss = amountMiss;
            AmountGeki = amountGeki;
            AmountKatu = amountKatu;
            Mods = mods;
        }

        public bool IsRelaxing()
        {
            return Mods.HasFlag(Mods.Relax) || Mods.HasFlag(Mods.Relax2);
        }

        public bool IsAutoplay()
        {
            return Mods.HasFlag(Mods.Autoplay);
        }

        public int TotalHits()
        {
            return Amount300 + Amount100 + Amount50 + AmountMiss;
        }

        public int TotalSuccessfulHits()
        {
            return Amount300 + Amount100 + Amount50;
        }

        public double Accuracy()
        {
            var totalHits = TotalHits();

            if (totalHits <= 0)
                return 0.0;

            var accuracy = (300 * Amount300 + 100 * Amount100 + 50 * Amount50) / (totalHits * 300);
            return Math.Max(Math.Min(accuracy, 1.0), 0.0);
        }
    }
}
