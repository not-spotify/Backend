namespace MusicPlayerBackend.Host

open System
open System.IO
open System.Text
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Options
open Microsoft.IdentityModel.JsonWebTokens
open Microsoft.IdentityModel.Tokens
open Microsoft.OpenApi.Models
open Serilog
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Minio

open MusicPlayerBackend
open MusicPlayerBackend.Common
open MusicPlayerBackend.Host
open MusicPlayerBackend.Host.Ext
open MusicPlayerBackend.Host.Services
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Identity

type Startup(config: IConfiguration) =
    member _.ConfigureServices(services: IServiceCollection) =
        %services
            .Configure<OptionSections.AppConfig>(config)
            .Configure<OptionSections.Minio>(config.GetSection("minio"))
            .Configure<OptionSections.TokenConfig>(config.GetSection(nameof(OptionSections.TokenConfig)))
            .Configure<PasswordHasherOptions>(fun (o: PasswordHasherOptions) -> o.IterationCount <- 600_000)

        %services
             .AddSerilog()
             .AddHttpContextAccessor()

        %services.AddMinio(fun minioClient ->
            let minioConfig = config.GetSection("minio").Get<OptionSections.Minio>()
            %minioClient
                .WithSSL(minioConfig.UseSsl)
                .WithEndpoint(minioConfig.Endpoint, minioConfig.Port)
                .WithCredentials(minioConfig.AccessKey, minioConfig.SecretKey))

        %services.AddScoped<FsharpAppDbContext>(fun c ->
            let optionsBuilder = DbContextOptionsBuilder<FsharpAppDbContext>()

            %optionsBuilder.UseNpgsql(
                connectionString = c.GetRequiredService<IConfiguration>().GetConnectionString(FsharpAppDbContext.ConnectionStringName),
                npgsqlOptionsAction = fun builder -> %builder.MigrationsAssembly(typeof<FsharpAppDbContext>.Assembly.GetName().Name)
            )

            new FsharpAppDbContext(optionsBuilder.Options)
        )

        %services
            .AddScoped<FsharpUnitOfWork>()
            .AddScoped<FsharpAlbumRepository>()
            .AddScoped<FsharpPlaylistRepository>()
            .AddScoped<FsharpPlaylistUserPermissionRepository>()
            .AddScoped<FsharpRefreshTokenRepository>()
            .AddScoped<FsharpTrackPlaylistRepository>()
            .AddScoped<FsharpTrackRepository>()
            .AddScoped<FsharpUserRepository>()

        %services
            .AddTransient<S3Service>()

        %services
            .AddControllers()
            .AddControllersAsServices()
            .AddJsonOptions(fun opts -> opts.JsonSerializerOptions.Converters.Add(JsonStringEnumConverter()))
            .AddXmlSerializerFormatters()

        %services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun o ->
                o.TokenValidationParameters <- TokenValidationParameters(
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection(nameof(OptionSections.TokenConfig)).Get<OptionSections.TokenConfig>().SigningKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireAudience = false
            )
        )

        %services
            .AddCustomIdentity()
            .AddAuthorization()
            .AddTransient<UserProvider>()
            .AddTransient<JwtService>()

        %services.AddEndpointsApiExplorer()

        %services.AddSwaggerGen(fun c ->
            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, OpenApiSecurityScheme(
                Type = SecuritySchemeType.Http,
                BearerFormat = JwtConstants.TokenType,
                In = ParameterLocation.Header,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Name = "Authorization",
                Description = "Please insert JWT into field"
            ))

            let oaRef = OpenApiReference(Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme)
            let openApiSecurityScheme = OpenApiSecurityScheme(Reference = oaRef)

            let securityRequirement = OpenApiSecurityRequirement()
            securityRequirement.Add(openApiSecurityScheme, Array.empty)

            c.AddSecurityRequirement(securityRequirement)
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Host.xml"))
            c.EnableAnnotations()
        )

    member _.Configure(app: IApplicationBuilder,
                       env: IWebHostEnvironment,
                       ctx: FsharpAppDbContext,
                       appConfig: IOptions<OptionSections.AppConfig>,
                       minioClient: IMinioClient) =

        %minioClient
            .CreateBucketIfNotExists("tracks")
            .CreateBucketIfNotExists("covers")

        if appConfig.Value.MigrateDatabaseOnStartup then
            ctx.Database.Migrate()

        %app.UseSerilogRequestLogging()
        if env.IsDevelopment() then
            %app.UseSwagger()
            %app.UseSwaggerUI(fun o -> o.DisplayOperationId())

        %app.UseMiddleware<UnauthorizedMiddleware>()
        %app
            .UseAuthentication()
            .UseRouting()
            .UseAuthorization()
            .UseEndpoints(fun e -> %e.MapControllers())
