using API;
using API.Data;
using API.Entities;
using API.Extensions;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(builder => builder
.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials() // SignalR related
.WithOrigins("https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence"); // SignalR related
app.MapHub<MessageHub>("hubs/message"); // SignalR related

using var scope = app.Services.CreateScope(); // Gives you access to all of the services within this Program class
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync(); // If you drop DB, this gives a new DB with refereshed seed data
    await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]"); // Removes unwanted conenctions from the db
    await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

app.Run();
