using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IEEE_Application.DATA;
using IEEE_Application.DATA.Models;
using IEEE_Application.Repository;
using IEEE_Application.Repository.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// ✅ SERVICE CONFIGURATION
// -------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS (Allow all origins for now — change later for production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ Database context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IUnitOfWork, MainUnitOfWork>();
builder.Services.AddScoped<ISpecialPuzzelRepository, SpecialPuzzelRepository>();
builder.Services.AddMemoryCache();

// ✅ Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ✅ Prevent redirect loops (API-friendly status codes)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// ✅ JWT Authentication
var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "https://puzzlers-api";
var jwtAudience = builder.Configuration["JWT:Audience"];
var jwtSecret = builder.Configuration["JWT:SecretKey"]
    ?? throw new InvalidOperationException("JWT:SecretKey is not configured.");
var validateAudience = !string.IsNullOrWhiteSpace(jwtAudience);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = validateAudience,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = validateAudience ? jwtAudience : null,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
    };
});

// -------------------------------------------------------
// ✅ BUILD APP
// -------------------------------------------------------

var app = builder.Build();

// -------------------------------------------------------
// ✅ INITIAL DATABASE CHECK + MIGRATIONS + SEED
// -------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();

    try
    {
        int retries = 0;
        const int maxRetries = 10;
        const int delayMs = 5000;

        while (!db.Database.CanConnect() && retries < maxRetries)
        {
            retries++;
            Console.WriteLine($"⏳ Waiting for SQL Server... attempt {retries}/{maxRetries}");
            Thread.Sleep(delayMs);
        }

        if (db.Database.CanConnect())
        {
            Console.WriteLine("✅ Connected successfully to the existing database.");

            // 🚀 Apply migrations
            Console.WriteLine("🔄 Applying database migrations...");
            db.Database.Migrate();
            Console.WriteLine("✅ Migrations applied successfully.");

            // 🧩 Seed admin role & user
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string roleName = "ADMIN";
            string userName = "root";
            string password = "Root123?";

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (roleResult.Succeeded)
                    Console.WriteLine($"✅ Role '{roleName}' created.");
            }

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new User { UserName = userName, Email = "root@example.com" };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                    Console.WriteLine($"✅ Admin user '{userName}' created and assigned to '{roleName}'.");
                }
                else
                {
                    Console.WriteLine($"⚠️ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine("ℹ️ Admin user already exists.");
            }
        }
        else
        {
            Console.WriteLine("❌ Could not connect to the database after several attempts. Check credentials or network.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("💥 Database initialization failed: " + ex.Message);
    }
}

// -------------------------------------------------------
// ✅ MIDDLEWARE PIPELINE (order matters!)
// -------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IEEE API V1");
    });
}

// 🔑 CORS must come before Authentication
app.UseCors("AllowAngularClient");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
