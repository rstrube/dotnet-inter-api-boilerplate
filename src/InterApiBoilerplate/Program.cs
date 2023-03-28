using System;
using InterApiBoilerplate.Clients;
using InterApiBoilerplate.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace InterApiBoilerplate;

public sealed class Program
{
    private static ILogger<Program>? _logger;
    
    public static int Main(string[] args)
    {
        var webAppBuilder = CreateWebAppBuilder(args);
        
        ConfigureServices(webAppBuilder);

        var app = webAppBuilder.Build();

        using(var serviceScope = app.Services.CreateScope())
        {
            var services = serviceScope.ServiceProvider;
            _logger = services.GetRequiredService<ILogger<Program>>();
        }

        try
        {
            ConfigureApp(app);

            _logger.LogInformation("Intermediate API Boilerplate successfully bootstrapped.");

            app.Run();

            _logger.LogInformation("Intermediate API Boilerplate stopped cleanly.");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Intermediate API Boilerplate unhandled exception.");

            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static WebApplicationBuilder CreateWebAppBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.ConfigureAppConfiguration((context, config) => {
            config.SetBasePath(context.HostingEnvironment.ContentRootPath);
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"appsettings.Secret.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();
        });

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services);
        });

        return builder;
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // grab the cors section and config object
		var corsSection = builder.Configuration.GetSection(CorsConfig.Section);
		var corsConfig = corsSection.Get<CorsConfig>();

        // configure CORS
        builder.Services.AddCors(corsOptions => ConfigureCorsOptions(corsOptions, corsConfig));

        // get a specific section from appSettings.json
        var boredClientSection = builder.Configuration.GetSection(BoredClientConfig.Section);

        // configure IOptions<BoredClientConfig> to use the section above when passed in via DI
        builder.Services.Configure<BoredClientConfig>(boredClientSection);

        // create a local instance of BoredClientConfig that can be used during application startup
        var boredClientConfig = boredClientSection.Get<BoredClientConfig>();

        builder.Services.AddSingleton<MockBoredClient>();
        builder.Services.AddSingleton<BoredClient>();
        builder.Services.AddSingleton<IBoredClient>(sp =>
        {
            return boredClientConfig.UseMock
             ? sp.GetRequiredService<MockBoredClient>()
             : sp.GetRequiredService<BoredClient>();
        });


        // add additional services here:
        //builder.Services.AddTransient<IMyService, MyService>();
        //builder.Services.AddScoped<IMyService2, MyService2>();

        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    private static void ConfigureCorsOptions(CorsOptions corsOptions, CorsConfig corsConfig)
    {
        if (corsConfig.AllowAnyOrigin)
        {
            corsOptions.AddDefaultPolicy(
                policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
        }

        else if (corsConfig.AllowedOrigins != null && corsConfig.AllowedOrigins.Length > 0)
        {
            corsOptions.AddDefaultPolicy(
                policy =>
                {
                    policy.WithOrigins(corsConfig.AllowedOrigins)
                          .SetIsOriginAllowedToAllowWildcardSubdomains()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
        }
    }

    private static void ConfigureApp(WebApplication app)
    {
        // Configure the HTTP request pipeline.

        // when running in development mode, enable swagger & swagger UI
        if (app.Environment.IsDevelopment())
        {
            // open API specs are available at https://localhost:<port>/swagger/v1/swagger.json
            app.UseSwagger();

            // swagger UI is available at https://localhost:<port>/swagger
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors();
        app.UseAuthorization();
        app.MapControllers();
    }
}