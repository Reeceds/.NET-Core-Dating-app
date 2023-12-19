using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentityCore<AppUser>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
        })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>() // Needs to be of type 'RoleManager' first
            .AddEntityFrameworkStores<DataContext>(); // Create all of the tables related to Inentity in the db

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => 
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            // For signalR authentication, bearerToken must be passed on query params as signalR does not use http so it cannot be passed as http header
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"]; // Gets the bearerToken, MUST specify "access_token" 
                    var path = context.HttpContext.Request.Path;

                    if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs")) // '/hubs' specified in Program.cs file
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        //! Always add Authorization AFTER Authentication
        services.AddAuthorization(opt => {
            // 'RequireAdminRole' & 'ModeratePhotoRole' are both specified in AdminController
            opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
            opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
        });

        return services;
    }
}
