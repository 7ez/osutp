using System.Text;
using osu.GameplayElements.Beatmaps;
using osu.GameplayElements.HitObjects;
using Newtonsoft.Json;
using osutp.TomPoints;
using Microsoft.AspNetCore.Mvc;

namespace osutp.Service
{
    public class DifficultyCalculationRequest
    {
        public BeatmapBase? Beatmap { get; set; }
        public List<HitObjectBase>? HitObjects { get; set; }
    }

    public class Difficulty
    {
        public static async Task<IResult> BeatmapDifficultyCalculation(HttpContext context)
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
                .DeserializeObject<DifficultyCalculationRequest>(json)
                ?? throw new JsonSerializationException("Failed to deserialize DifficultyCalculationRequest.");

            var calculator = () => new TpDifficulty().Process(request.Beatmap, request.HitObjects);
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
