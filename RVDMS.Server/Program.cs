using Microsoft.OpenApi.Models;
using RVDMS.Api.Middleware;
using RVDMS.Application;
using RVDMS.Infrastructure;
using RVDMS.Infrastructure.Data;
using RVDMS.Infrastructure.Seeders.MasterSeeder;
using Serilog;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting up the application...");

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllers();
    
    // CORS configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            policy =>
            {
                policy.WithOrigins("https://localhost:64900",
                    "https://rvdms-web.netlify.app")
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
            Description = @"
                <strong>🏛️ Regional Visual Dashboard Management System</strong><br/>
                Official API for the State Department for Housing and Urban Development.
                
                <h3>🔑 Key Features:</h3>
                <ul>
                    <li><strong>Projects:</strong> AHP, ESP Markets, Institutional Housing, Urban Development</li>
                    <li><strong>Progress Tracking:</strong> Physical progress, time elapsed, variance analysis</li>
                    <li><strong>Geo-Validation:</strong> Location-based report verification</li>
                    <li><strong>Role-Based Access:</strong> RL, TL, CS, CDH, COW, SuperAdmin</li>
                    <li><strong>Real-time Analytics:</strong> Project status, delays, completions</li>
                </ul>

                <h3>📋 Implementation Notes:</h3>
                <ul>
                    <li>All endpoints require authentication via Bearer token</li>
                    <li>Responses are paginated with metadata</li>
                    <li>Dates are returned in ISO 8601 format (UTC)</li>
                    <li>Progress values are percentages (0-100)</li>
                </ul>

                <h3>📞 Support:</h3>
                <ul>
                    <li>Technical Support: <a href='mailto:support@rvdms.go.ke'>support@rvdms.go.ke</a></li>
                    <li>Documentation: <a href='https://docs.rvdms.go.ke'>https://docs.rvdms.go.ke</a></li>
                    <li>Status Page: <a href='https://status.rvdms.go.ke'>https://status.rvdms.go.ke</a></li>
                </ul>
            ",
            TermsOfService = new Uri("https://rvdms.go.ke/terms"),
            Contact = new OpenApiContact
            {
                Name = "RVDMS Technical Team",
                Email = "support@rvdms.go.ke",
                Url = new Uri("https://rvdms.go.ke")
            },
            License = new OpenApiLicense
            {
                Name = "Government Open Data License v1.0",
                Url = new Uri("https://rvdms.go.ke/license")
            }
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
    // DATABASE MIGRATION & SEEDING (FIXED)
    // ============================================================
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        
        try
        {
            // Step 1: Apply migrations to create tables
            Log.Information("Checking if database migrations need to be applied...");
            
            if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                Log.Information("Applying pending migrations...");
                await dbContext.Database.MigrateAsync();
                Log.Information("Migrations applied successfully!");
            }
            else
            {
                Log.Information("No pending migrations. Database is up to date.");
            }
            
            // Step 2: Ensure database is created (if no migrations exist)
            var canConnect = await dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                Log.Information("Database doesn't exist. Creating...");
                await dbContext.Database.EnsureCreatedAsync();
                Log.Information("Database created successfully!");
            }
            
            // Step 3: Seed the database
            Log.Information("Starting database seeding...");
            await initializer.SeedAsync();
            Log.Information("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating or seeding the database!");
            // Don't throw - let the app continue
            // The app will still start, but with possible missing data
        }
    }

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
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

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
