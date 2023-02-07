namespace InterApiBoilerplate.Configuration;

public class CorsConfig
{
    public const string Section = "Cors";
    public string[]? AllowedOrigins { get; set; }
    public bool AllowAnyOrigin { get; set; }
}