using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkDir.API.Entities;
using WorkDir.API.Models.DataModels;
using WorkDir.API.Models;
using WorkDir.API.Services.Authentication;
using WorkDir.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WorkDir.API;

public static class StartupServicesHelper
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnectionString = configuration.GetConnectionString("WorkDirDbConnectionString");

        services.AddDbContext<WorkContext>(options => 
        {
            options.UseSqlServer(dbConnectionString);
        });

        return services;   
    }

    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authenticationSettings = new AuthenticationSettings();
        configuration.GetSection("Authentication").Bind(authenticationSettings);

        services.AddSingleton(authenticationSettings);

        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = "Bearer";
            option.DefaultScheme = "Bearer";
            option.DefaultChallengeScheme = "Bearer";
        })
            .AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = authenticationSettings.JwtIssuer,
                    ValidAudience = authenticationSettings.JwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)),
                };
            });

        return services;
    }

    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IValidator<UserRegisterDto>, RegisterUserDtoValidator>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }



}
