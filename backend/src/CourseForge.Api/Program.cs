
using System.Text;
using CourseForge.Application;
using CourseForge.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Configuration
// ==========================================

builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true
);

// ==========================================
// 2. Controllers and Swagger
// ==========================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// ==========================================
// 3. Database Configuration - SQL Server
// ==========================================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"));
});

// ==========================================
// 4. Dependency Injection
// ==========================================

// JWT Token Service
builder.Services.AddScoped<ITokenService, TokenService>();

// Google OAuth Identity Verification
builder.Services.AddScoped<
    IGoogleIdentityVerifier,
    GoogleIdentityVerifier>();

// Razorpay Payment Gateway
builder.Services.AddHttpClient<
    IPaymentGateway,
    RazorpayGateway>();

// Password Hashing
builder.Services.AddScoped<
    IPasswordHasher<CourseForge.Domain.User>,
    PasswordHasher<CourseForge.Domain.User>>();

// ==========================================
// 5. JWT Authentication Configuration
// ==========================================

var jwtKey = builder.Configuration["Jwt:Key"] ?? "";

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Configure Jwt:Key (at least 32 bytes) " +
        "using appsettings.Local.json or " +
        "environment variables.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidateAudience = true,

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });

// ==========================================
// 6. Authorization
// ==========================================

builder.Services.AddAuthorization();

// ==========================================
// 7. CORS Configuration - React Frontend
// ==========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration["FrontendUrl"]
                ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ==========================================
// 8. Build Application
// ==========================================

var app = builder.Build();

// ==========================================
// 9. HTTP Request Pipeline
// ==========================================

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("frontend");

// Authentication Middleware
app.UseAuthentication();

// Authorization Middleware
app.UseAuthorization();

// Map API Controllers
app.MapControllers();

// ==========================================
// 10. Run Application
// ==========================================

app.Run();
