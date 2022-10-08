using BffBoilerplate.Clients;
using BffBoilerplate.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BffBoilerplate;

internal sealed class Program
{
    static void Main(string[] args)
    {
        var builder = CreateHostBuilder(args);

        ConfigureServices(builder);

        var app = builder.Build();

        ConfigureApp(app);

        app.Run();
    }

    static WebApplicationBuilder CreateHostBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.ConfigureAppConfiguration((context, config) => {
            config.SetBasePath(context.HostingEnvironment.ContentRootPath);
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"appsettings.Secret.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();
        });

        return builder;
    }

    static void ConfigureServices(WebApplicationBuilder builder)
    {
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

    static void ConfigureApp(WebApplication app)
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
        app.UseAuthorization();
        app.MapControllers();
    }
}