
using EmployeeManagementSystem.Repositories;
using EmployeeManagementSystem.Security;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.Middleware;

using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Microsoft.AspNetCore.RateLimiting;

using System.Text;
using System.Threading.RateLimiting;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing")))
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://eigenvuefrontend.azurewebsites.net")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Rate Limiter
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthLimit", windowOptions =>
    {
        windowOptions.PermitLimit = 5;
        windowOptions.Window = TimeSpan.FromMinutes(1);
        windowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        windowOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Controllers + OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// PostgreSQL Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

var app = builder.Build();

// Custom Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// OpenAPI
app.MapOpenApi();
app.MapScalarApiReference();

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

    await next();
});

app.UseHttpsRedirection();

app.UseCors("AllowVueApp");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    if (!context.Users.Any(u => u.Role == "Admin"))
    {
        context.Users.Add(new EmployeeManagementSystem.Models.User
        {
            Username = "admin",
            Password = passwordHasher.HashPassword("Admin@123"),
            Role = "Admin"
        });
        context.SaveChanges();
    }
}

app.Run();
