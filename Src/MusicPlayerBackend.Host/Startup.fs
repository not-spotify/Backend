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
open MusicPlayerBackend.App.Middlewares
open MusicPlayerBackend.Data
open MusicPlayerBackend.Data.Entities
open MusicPlayerBackend.Data.Identity.Stores
open MusicPlayerBackend.Data.Repositories
open MusicPlayerBackend.Identity
open MusicPlayerBackend.Services
open Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure
open Serilog
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Minio
open MusicPlayerBackend.App

open MusicPlayerBackend.Common.TypeExtensions
open MusicPlayerBackend.Options

type Startup(config: IConfiguration) =
    member _.ConfigureServices(services: IServiceCollection) =
        %services
            .Configure<AppConfig>(config)
            .Configure<MusicPlayerBackend.Options.Minio>(config.GetSection(nameof(Minio)))
            .Configure<TokenConfig>(config.GetSection(nameof(TokenConfig)))
            .Configure<PasswordHasherOptions>(fun (o: PasswordHasherOptions) -> o.IterationCount <- 600_000)

        %services
             .AddSerilog()
             .AddHttpContextAccessor()

        %services.AddMinio(fun o ->
            let minioConfig = config.GetSection("Minio").Get<MusicPlayerBackend.Options.Minio>()
            %o
                .WithSSL(minioConfig.UseSsl)
                .WithEndpoint(minioConfig.Endpoint, minioConfig.Port)
                .WithCredentials(minioConfig.AccessKey, minioConfig.SecretKey))

        %services.AddScoped<AppDbContext>(fun c ->
            let optionsBuilder = DbContextOptionsBuilder<AppDbContext>()

            %optionsBuilder.UseNpgsql(
                connectionString = c.GetRequiredService<IConfiguration>().GetConnectionString(AppDbContext.ConnectionStringName),
                npgsqlOptionsAction = Action<NpgsqlDbContextOptionsBuilder>(fun b -> %b.MigrationsAssembly(typeof<AppDbContext>.Assembly.GetName().Name))
            )

            new AppDbContext(optionsBuilder.Options)
        )

        %services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IAlbumRepository, AlbumRepository>()
            .AddScoped<IPlaylistRepository, PlaylistRepository>()
            .AddScoped<IPlaylistUserPermissionRepository, PlaylistUserPermissionRepository>()
            .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
            .AddScoped<ITrackPlaylistRepository, TrackPlaylistRepository>()
            .AddScoped<ITrackRepository, TrackRepository>()
            .AddScoped<IUserRepository, UserRepository>()

        %services
            .AddTransient<IS3Service, S3Service>()

        %services
            .AddControllers()
            .AddControllersAsServices()
            .AddJsonOptions(fun opts -> opts.JsonSerializerOptions.Converters.Add(JsonStringEnumConverter()))
            .AddXmlSerializerFormatters()

        %services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun o ->
                o.TokenValidationParameters <- TokenValidationParameters(
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection(nameof(TokenConfig)).Get<TokenConfig>().SigningKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireAudience = false
            )
        )

        %services.AddScoped<IdentityErrorDescriber>()
        %services.AddScoped<ILookupNormalizer, LookupNormalizer>()
        %services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>()
        %services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>()
        %services.AddScoped<IUserConfirmation<User>, UserConfirmation>()
        %services.AddScoped<IUserStore<User>, UserStore>()
        %services.AddScoped<IUserValidator<User>, UserValidator<User>>()
        %services.AddScoped<UserManager<User>>()
        %services.AddScoped<SignInManager<User>>()

        %services.AddAuthorization()
        %services.AddTransient<IUserProvider, UserProvider>()

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

            let xmlFilename = "MusicPlayerBackend.xml"
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename))
            c.EnableAnnotations()
        )

    member _.Configure(app: IApplicationBuilder,
                       env: IWebHostEnvironment,
                       ctx: AppDbContext,
                       appConfig: IOptions<AppConfig>,
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
            .UseRouting()
            .UseAuthorization()
            .UseAuthentication()
            .UseEndpoints(fun e -> %e.MapControllers())
