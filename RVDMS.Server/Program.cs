using Microsoft.OpenApi.Models;
using RVDMS.Api.Middleware;
using RVDMS.Application;
using RVDMS.Infrastructure;
using RVDMS.Infrastructure.Seeders.MasterSeeder;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://localhost:64900",
                "https://rvdms.netlify.app")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.0",
        Title = "RVDMS API",
        Description = @"...", // Your description here
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer' [space] and then your token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Host.UseSerilog();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// ============================================================
// FIXED: Seeding with proper error handling
// ============================================================
try
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.SeedAsync();
    Log.Information("Database seeding completed successfully!");
}
catch (Exception ex)
{
    Log.Error(ex, "FATAL ERROR: Database seeding failed!");
    // Don't re-throw - let the app continue, but log the error
    // The app will still start, but with empty data
}

// ============================================================
// Swagger - Production safe
// ============================================================
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RVDMS API v1");
        options.RoutePrefix = "swagger";
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    message = "RVDMS API is running",
    swagger = "/swagger",
    environment = app.Environment.EnvironmentName,
    status = "healthy"
}));

// Only fallback to index.html if file exists (remove if not needed)
if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html")))
{
    app.MapFallbackToFile("/index.html");
}

app.Run();
