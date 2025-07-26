namespace osutp.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapPost("/difficulty", Difficulty.BeatmapDifficultyCalculation).WithName("BeatmapDifficultyCalculation");
            app.MapPost("/performance", Performance.CalculateScorePerformance).WithName("CalculateScorePerformance");
            app.Run();
        }
    }
}
