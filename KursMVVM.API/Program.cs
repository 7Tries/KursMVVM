using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using KursMVVM.API.Models;
using KursMVVM.API.Models.Data;

var builder = WebApplication.CreateBuilder(args);

// ─── Authentication & Authorization ─────────────────────────────────────────
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = AuthOptions.ISSUER,
            ValidateAudience         = true,
            ValidAudience            = AuthOptions.AUDIENCE,
            ValidateLifetime         = true,
            IssuerSigningKey         = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });

// ─── Controllers & API Explorer ─────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ─── Swagger ─────────────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "KursMVVM REST API",
        Version     = "v1",
        Description = "REST API для управления сотрудниками, договорами, поставками и организациями."
    });

    // Поддержка JWT в Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Формат: Bearer {token}",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ─── Memory Cache ─────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions
{
    TrackStatistics = true,
    SizeLimit       = 200,
}));

// ─── Database (SQLite — тот же файл, что и в основном проекте) ───────────────
var dbPath = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? "Data Source=kurs.db";
builder.Services.AddDbContext<KursContext>(opt => opt.UseSqlite(dbPath));

// ─── CORS & Response Caching ─────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddResponseCaching();

var app = builder.Build();

// ─── Создать БД при первом запуске ───────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KursContext>();
    db.Database.EnsureCreated();
}

// ─── Middleware Pipeline ──────────────────────────────────────────────────────
app.UseRouting();

app.UseCors();
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KursMVVM API v1");
        c.RoutePrefix = string.Empty; // Swagger UI на корне: http://localhost:PORT/
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
