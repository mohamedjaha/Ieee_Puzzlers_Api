using System;
using System.Text;
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
// ✅ SERVICES CONFIGURATION
// -------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS — allow Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ✅ Database
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
var jwtSecret = builder.Configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey is not configured.");
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
// ✅ INITIAL DB CHECK (optional retry loop)
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

            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            string roleName = "ADMIN";
            string userName = "root";
            string password = "Root123?";

            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new User { UserName = userName, Email = "root@example.com" };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, roleName);
            }
        }
        else
        {
            Console.WriteLine("❌ Could not connect to DB after several attempts.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("💥 Database initialization failed: " + ex.Message);
    }
}

// -------------------------------------------------------
// ✅ MIDDLEWARE ORDER (IMPORTANT!)
// -------------------------------------------------------

// ⚙️ Enable Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IEEE API V1");
    });
}

// ✅ Enable CORS BEFORE authentication/authorization
app.UseCors("AllowAngularClient");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
