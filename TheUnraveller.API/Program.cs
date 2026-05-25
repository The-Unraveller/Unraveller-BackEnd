using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Infrastructure.Repositories;
using TheUnraveller.Service.Implementations;
using TheUnraveller.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=unraveller.db"));

// Register Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMissionRepository, MissionRepository>();
builder.Services.AddScoped<IDialogueRepository, DialogueRepository>();
builder.Services.AddScoped<IUserProgressRepository, UserProgressRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Register Services
builder.Services.AddScoped<IGameEngineService, GameEngineService>();
builder.Services.AddScoped<IMissionService, MissionService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpClient<ILLMProviderService, LlmProviderService>();
// builder.Services.AddScoped<IUserService, UserService>(); // Implementation omitted for brevity

// CORS for Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "The Unraveller API V1");
    c.RoutePrefix = string.Empty; // Swagger sẽ hiển thị ngay tại trang chủ (root)
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Ensure Database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
