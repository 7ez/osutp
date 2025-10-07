namespace osutp.Service;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IncludeFields = true;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        var app = builder.Build();
        app.MapControllers();
        app.Run();
    }
}