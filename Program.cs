using IEEE_Application.DATA;
using IEEE_Application.DATA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IEEE_Application.Repository.Base;
using IEEE_Application.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add essential services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication();

// ✅ STEP 0: Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // your Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
// only if using cookies or auth headers
    });
});

// Connect to SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection1"));
});

builder.Services.AddScoped<IUnitOfWork, MainUnitOfWork>();

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// ✅ STEP 1: Ensure database is created / migrated before using Identity
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();

        // ✅ STEP 2: Create default role and admin user
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
    catch (Exception ex)
    {
        Console.WriteLine("❌ Database migration or seeding failed: " + ex.Message);
    }
}

// Swagger UI only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IEEE API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ✅ STEP 3: Apply the CORS policy BEFORE MapControllers()
app.UseCors("AllowAngularClient");

app.MapControllers();

app.Run();
