using Newtonsoft.Json;
using System.Text;
using osutp.TomPoints;

namespace osutp.Service
{
    public class PerformanceCalculationRequest
    {
        public TpDifficultyCalculation? Difficulty { get; set; }
        public TpScore? Score { get; set; }
    }

    public class Performance
    {
        public static async Task<IResult> CalculateScorePerformance(HttpContext context)
        {
            context.Request.EnableBuffering();
            context.Request.Body.Position = 0;
            string json;

            using (var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true))
            {
                json = await reader.ReadToEndAsync().ConfigureAwait(false);
                context.Request.Body.Position = 0;
            }

            var request = JsonConvert
                .DeserializeObject<PerformanceCalculationRequest>(json)
                ?? throw new JsonSerializationException("Failed to deserialize DifficultyCalculationRequest.");

            var calculator = () => new TpPerformance(request.Difficulty, request.Score).ComputeTotalValue();
            var calculation = await Task.Run(calculator, context.RequestAborted).ConfigureAwait(false);
            if (calculation == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Results.BadRequest();
            }

            var calculationEncoded = JsonConvert.SerializeObject(calculation);
            if (calculationEncoded == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Results.BadRequest();
            }

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(calculationEncoded);
            return Results.Ok();
        }
    }
}
