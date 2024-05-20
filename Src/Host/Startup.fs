namespace MusicPlayerBackend.Host

open System
open System.IO
open System.Reflection
open System.Text.Json.Serialization
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.IdentityModel.JsonWebTokens
open Microsoft.OpenApi.Models
open MusicPlayerBackend.Persistence.Stores
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

        %services
            .AddTransient<S3Service>()

        %services.AddSingleton<Domain.User.UserStore> (fun _ -> InMemoryUserStore.create())
        %services.AddSingleton<Domain.Shared.Bus> (fun _ ->
            {
                Publish = fun _ -> Task.CompletedTask
            } : Domain.Shared.Bus)

        %services.AddSingleton<Domain.User.CreateUser> (System.Func<IServiceProvider, _>(
            fun sc ->
                let userStore = sc.GetRequiredService<Domain.User.UserStore>()
                let bus = sc.GetRequiredService<Domain.Shared.Bus>()
                Domain.User.createUser userStore bus
        ))

        %services.AddSingleton<Domain.Playlist.PlaylistStore> (fun _ -> InMemoryPlaylistStore.create())

        %services.AddSingleton<Domain.Playlist.CreatePlaylist> (System.Func<IServiceProvider, _>(
            fun sc ->
                let playlistStore = sc.GetRequiredService<Domain.Playlist.PlaylistStore>()
                let bus = sc.GetRequiredService<Domain.Shared.Bus>()
                Domain.Playlist.createPlaylist playlistStore bus
        ))

        %services
            .AddControllers()
            .AddControllersAsServices()
            .AddJsonOptions(fun opts -> opts.JsonSerializerOptions.Converters.Add(JsonStringEnumConverter()))
            .AddXmlSerializerFormatters()
            .AddJsonOptions(fun options ->
                JsonFSharpOptions.Default()
                    .WithUnionUntagged()
                    .AddToJsonSerializerOptions(options.JsonSerializerOptions))

        %services.AddEndpointsApiExplorer()

        %services.AddSwaggerGen(fun c ->
            c.SupportNonNullableReferenceTypes()
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
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"))
            c.EnableAnnotations()
        )

    member _.Configure(app: IApplicationBuilder,
                       env: IWebHostEnvironment,
                       minioClient: IMinioClient) =

        %minioClient
            .CreateBucketIfNotExists("tracks")
            .CreateBucketIfNotExists("covers")

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
