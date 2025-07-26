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

        public byte ApproachRate = 5;
        public byte CircleSize = 5;
        public byte HpDrainRate = 5;
        public byte OverallDifficulty = 5;

        public double SliderMultiplier = 1.4;
        public double SliderTickRate = 1;
    }
}
