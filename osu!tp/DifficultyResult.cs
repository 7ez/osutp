namespace osu_tp.TomPoints
{
    public class TpDifficultyCalculation
    {
        public int AmountNormal;
        public int AmountSliders;
        public int AmountSpinners;
        public int MaxCombo;

        public double SpeedDifficulty;
        public double AimDifficulty;
        public double SpeedStars;
        public double AimStars;
        public double StarRating;

        public float ApproachRate = 5;
        public float CircleSize = 5;
        public float HpDrainRate = 5;
        public float OverallDifficulty = 5;

        public double SliderMultiplier = 1.4;
        public double SliderTickRate = 1;
    }
}
