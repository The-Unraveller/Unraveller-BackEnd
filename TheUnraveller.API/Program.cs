using TheUnraveller.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PayOS;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Infrastructure.Repositories;
using TheUnraveller.Service.Implementations;
using TheUnraveller.Service.Interfaces;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Load environment variables from .env file if it exists
var currentDir = Directory.GetCurrentDirectory();
var envFiles = new[] 
{ 
    Path.Combine(currentDir, ".env"),
    Path.Combine(currentDir, "..", ".env"),
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env")
};

foreach (var envFilePath in envFiles)
{
    if (File.Exists(envFilePath))
    {
        foreach (var line in File.ReadAllLines(envFilePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                var envKey = parts[0].Trim();
                var envVal = parts[1].Trim();
                if ((envVal.StartsWith("\"") && envVal.EndsWith("\"")) || (envVal.StartsWith("'") && envVal.EndsWith("'")))
                {
                    envVal = envVal[1..^1];
                }
                Environment.SetEnvironmentVariable(envKey, envVal);
            }
        }
        break; // Stop at the first .env file found
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

// Configure DbContext (PostgreSQL - Supabase)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")!)
           .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// Register Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMissionRepository, MissionRepository>();
builder.Services.AddScoped<IDialogueRepository, DialogueRepository>();
builder.Services.AddScoped<IUserProgressRepository, UserProgressRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IShopRepository, ShopRepository>();

// Register Services
builder.Services.AddScoped<IGameEngineService, GameEngineService>();
builder.Services.AddScoped<IAIEvaluationService, AIEvaluationService>();
builder.Services.AddScoped<IMissionService, MissionService>();
builder.Services.AddScoped<IMissionManagementService, MissionManagementService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddHttpClient<ILLMProviderService, LlmProviderService>(client => client.Timeout = TimeSpan.FromSeconds(120));
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();

// Register payOS Client (singleton — thread-safe)
builder.Services.AddSingleton<PayOSClient>(sp =>
{
    var cfg = builder.Configuration.GetSection("PayOS");
    return new PayOSClient(cfg["ClientId"]!, cfg["ApiKey"]!, cfg["ChecksumKey"]!);
});

// CORS for Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate(); // Apply pending migrations
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB Migration Note]: {ex.Message}");
    }

    try
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""Feedbacks"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""UserId"" INT NOT NULL REFERENCES ""Users""(""Id"") ON DELETE CASCADE,
                ""Rating"" INT NOT NULL DEFAULT 5,
                ""Category"" VARCHAR(255) NOT NULL DEFAULT 'Trải nghiệm UI/UX',
                ""Comment"" TEXT NOT NULL DEFAULT '',
                ""CreatedAt"" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE INDEX IF NOT EXISTS ""IX_Feedbacks_UserId"" ON ""Feedbacks"" (""UserId"");
            CREATE INDEX IF NOT EXISTS ""IX_Feedbacks_CreatedAt"" ON ""Feedbacks"" (""CreatedAt"");

            UPDATE ""Missions"" SET ""Locked"" = FALSE WHERE ""Id"" <= 13;
            UPDATE ""Missions"" SET ""Locked"" = TRUE WHERE ""Id"" >= 14;
        ");
        Console.WriteLine("[DB Startup] Feedbacks table checked and synchronized on Supabase PostgreSQL.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB Startup Update] Note: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "The Unraveller API V1");
    c.RoutePrefix = string.Empty;
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ModeratorMiddleware>();
app.UseMiddleware<AdminMiddleware>();

app.MapControllers();

app.Run();
