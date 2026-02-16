using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.DependencyInjection;
using Kk.Kharts.Api.Middlewares;
using Kk.Kharts.Api.Policies;
using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.BackgroundServices;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utility;
using Kk.Kharts.Shared.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;

namespace Kk.Kharts.Api
{

    public class Program
    {
        private const string VERSION = "v1.1.0";
        private const string CorsPolicyName = "KkSecureCors";

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configura a resposta padrão para erros de validação de modelos.
            // Esta configuração substitui o retorno padrão do ASP.NET (ValidationProblemDetails)
            // por uma resposta mais limpa, contendo apenas os campos desejados:
            // status, title e errors (sem incluir "type" ou "traceId").
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState

                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            e => e.Key,
                            e => e.Value?.Errors.Select(x => x.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        );

                    var customResponse = new
                    {
                        status = StatusCodes.Status400BadRequest,
                        title = "Une ou plusieurs erreurs de validation se sont produites.",
                        errors
                    };

                    return new BadRequestObjectResult(customResponse);
                };
            });

            // Serviços para o container
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDbContext<AppDbContext>((sp, opts) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                opts.UseSqlServer(
                    config.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 6,                                // Retries moderados para não saturar o pool
                            maxRetryDelay: TimeSpan.FromSeconds(5),          // Backoff curto; camadas superiores cuidam de esperas longas
                            errorNumbersToAdd: null
                        );

                        sqlOptions.CommandTimeout(30);                       // Timeout de 30 segundos
                    });
            });

            // Telegram Bot Service (usando Telegram.Bot package)
            builder.Services.AddTelegramServices(builder.Configuration);

            // Adicionar política CORS para Mini App
            builder.Services.AddMiniAppPolicy();

            builder.Services.AddHostedService<StartupNotificationService>();

            // Convention-based registration: I{Name} → {Name}
            // Lifetime defaults to Scoped; use [SingletonService] or [TransientService] on the class to override.
            builder.Services.AddApplicationServices();

            // Manual registrations — name mismatch or special configuration
            builder.Services.AddScoped<IEmailService, SmtpEmailService>();

            builder.Services.AddHostedService<DeviceMonitorService>();
            builder.Services.AddHostedService<DailyLogProcessorService>();
            builder.Services.AddHostedService<AlarmEvaluatorService>();

            builder.Services.AddHttpClient<IPushoverService, PushoverService>(client =>
            {
                client.BaseAddress = new Uri("https://api.pushover.net/1/");
            });

            builder.Services.AddNotificationSystem();

            builder.Services.AddControllers();

            // Configuração da chave JWT vinda do appsettings.json

            var key = builder.Configuration["Jwt:key"];

            // Swagger para a documentação da API
            builder.Services.AddEndpointsApiExplorer();

            // Serviço para inicializar o banco de dados
            builder.Services.AddTransient<SeedDb>();

            // Configuração da autenticação usando JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 options.RequireHttpsMetadata = false;
                 options.SaveToken = true;
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true,
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ClockSkew = TimeSpan.Zero,
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
                     ValidIssuer = JwtConstants.Issuer,
                     ValidAudience = JwtConstants.Audience
                 };

                 options.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = context =>
                     {
                         var jwtLogger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                             .CreateLogger("JwtAuthentication");
                         jwtLogger.LogWarning(context.Exception, "Erreur d'authentification JWT");
                         return Task.CompletedTask;
                     }
                 };
             });

            //Configuração de CORS para permitir origens específicas(se necessário)
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ??
                [
                    "https://www.kropkontrol.com",
                    "https://kropkontrol.com",
                    "http://localhost:5175",
                ];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                          .WithHeaders(
                              HeaderNames.ContentType,
                              HeaderNames.Authorization,
                              HeaderNames.Accept,
                              "X-Requested-With")
                          .AllowCredentials();
                });

            });

            builder.Services.AddSwaggerGen(c =>
            {
                // Versão com data de build
                var assembly = Assembly.GetExecutingAssembly();
                var filePath = assembly.Location;
                var lastWriteTime = File.GetLastWriteTime(filePath);
                var versionWithDate = $"{VERSION} ( build: {lastWriteTime:dd-MM-yyyy - HH:mm:ss} )";

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = versionWithDate,
                    Title = "API KropKontrol",
                    Description = """

                        ## API de gestion des capteurs IoT pour l'agriculture de précision                        

                        Cette API permet de:

                        - 🔐 **Authentification** : Connexion JWT et gestion des tokens

                        - 📡 **Dispositifs** : Gestion des capteurs LoRaWAN (EM300-TH, EM300-DI, UC502)

                        - 🌡️ **Données** : Récupération des mesures (température, humidité, VWC, EC)

                        - 🏢 **Entreprises** : Gestion multi-tenant des sociétés

                        - ⚠️ **Alarmes** : Configuration des seuils d'alerte
                        

                        ### Authentification

                        - **JWT Bearer** : Pour les utilisateurs authentifiés

                        - **API Key** : Pour les passerelles IoT (header `KropKontrol`)
                        

                        ### Support

                        Pour toute question, contactez info@kropkontrol.com

                        ### Session Swagger

                        <a href="/swagger/logout" style="display:inline-flex;align-items:center;gap:6px;padding:6px 12px;border-radius:999px;background:#ef4444;color:#fff;text-decoration:none;font-weight:600;">🚪 Logout Swagger</a>

                        """,

                    Contact = new OpenApiContact
                    {
                        Name = "KropKontrol - API Developer",
                        Email = "info@kropkontrol.com",
                    },
                });

                // ============================================================

                // CONFIGURAÇÃO DE SEGURANÇA - JWT Bearer (Swashbuckle v10 - Documentação Oficial)

                // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v10.0.0/docs/configure-and-customize-swaggergen.md

                // ============================================================

                c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme. Entrez uniquement le token (sans 'Bearer')."
                });

                // Requisito de segurança global para JWT (Swashbuckle v10 usa delegate)
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("bearer", document)] = []
                });

                // ============================================================
                // CONFIGURAÇÃO DE SEGURANÇA - API Key
                // ============================================================

                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "Clé API dans l'en-tête KropKontrol",
                    Name = "KropKontrol",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });


                // Inclure les commentaires XML pour la documentation Swagger
                var xmlFilename = $"{assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Filtros de operação
                c.OperationFilter<DeviceThresholdsRequestExampleFilter>();
            });

            var app = builder.Build();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // CORS - aplica política segura para origens conhecidas
            app.UseCors(CorsPolicyName);

            app.UseHttpsRedirection();
            app.UseRouting();             // Precisa vir antes de autenticação/autorização
            app.UseAuthentication();      // Depois: autenticação por token -  // Usar a autenticação JWT
            app.UseAuthorization();       // Por fim: autorização de roles - // Habilitar autorização



            // ── Swagger protegido por login (SwaggerAuth no appsettings) ──
            // O middleware exige credenciais antes de mostrar a UI.
            // Após autenticação, o utilizador ainda precisa de um token JWT para chamar os endpoints.

            app.UseMiddleware<SwaggerAuthMiddleware>();
            {
                var assembly = Assembly.GetExecutingAssembly();
                var lastWriteTime = File.GetLastWriteTime(assembly.Location);
                var versionWithDate = $"{VERSION} ( build: {lastWriteTime:dd-MM-yyyy - HH:mm:ss} )";

                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"API {versionWithDate}");
                    c.OAuthClientId("swagger-ui");
                    c.OAuthAppName("Swagger UI");
                });
            }

            app.UseMiddleware<DebugUserActivityMiddleware>(); // depois da autenticação e autorização
            //app.UseMiddleware<CompanyMiddleware>();
            //app.UseMiddleware<PostHogTrackingMiddleware>(); PostHog para rastrear rotas acessadas
            app.MapControllers();

            // Configurer les commandes du bot Telegram au démarrage
            var telegramService = app.Services.GetRequiredService<ITelegramService>();
            await telegramService.SetBotCommandsAsync();
            await app.RunAsync();
        }
    }
}

