using System.Text.Json.Serialization;
namespace MusicPlayerBackend;

public sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddControllersAsServices().AddJsonOptions(opts =>
        {
            var enumConverter = new JsonStringEnumConverter();
            opts.JsonSerializerOptions.Converters.Add(enumConverter);
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseHttpsRedirection();
    }
}