using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using WorkDir.API.Middleware;
using WorkDir.API.Services;
using WorkDir.API.Services.Authentication;
using WorkDir.API.Services.StorageServices;
using WorkDir.Storage;
using WorkDir.API;

var builder = WebApplication.CreateBuilder(args);

//[*] DataBase Connection Configuration
builder.Services.AddDbContext(builder.Configuration);

//[*] Authentication Setup
builder.Services.ConfigureAuthentication(builder.Configuration);

//[*] AutoMaper Config
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddAuthServices();

builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddAzureStorageService();

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ISharingService, SharingService>();

builder.Services.AddControllers();

//[1/2] Error Handling MiddleWare
builder.Services.AddScoped<ErrorHandlingMiddleware>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Cors Settings
builder.Services.AddCors(options =>
{
    options.AddPolicy(name:"AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyMethod();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyOrigin();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.EnableTryItOutByDefault();
});

//[2/2] Error Handling MiddleWare
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();

app.UseHttpsRedirection();

//Cors
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();