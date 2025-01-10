using System.Text;
using DotNetEnv;
using MessengerApplication.Hubs;
using MessengerApplication.Models;
using MessengerApplication.Services;
using MessengerApplication.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
Env.Load();
builder.Configuration.AddEnvironmentVariables(); 
var origins = "*";
// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
        if (origins is not { Length: > 0 }) return;

        if (origins.Contains('*'))
        {
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
            builder.SetIsOriginAllowed(host => true);
            builder.AllowCredentials();
        }
        else
        {
            builder.WithOrigins(origins);
        }
        builder.WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MessengerApplicationDatabaseSettings>(
    builder.Configuration.GetSection("MessengerApplicationDatabase"));
builder.Services.AddScoped<IUsersService,UsersService>();
builder.Services.AddScoped<IChatsService,ChatsService>();
builder.Services.AddScoped<IMessagesService,MessagesService>();
builder.Services.AddScoped<DatabaseProviderService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<ChatHub>();
builder.Services.AddScoped(provider => new Lazy<IChatsService>(provider.GetRequiredService<IChatsService>));
builder.Services.AddScoped(provider => new Lazy<IMessagesService>(provider.GetRequiredService<IMessagesService>));
builder.Services.AddRouting();
builder.Services.AddSingleton<ConnectionMapping>();
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "Messenger",
        ValidAudience = "Messenger",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("mWwmSqZYUBXZCtGgWB9XjiWdMlhCFjJ9"))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Kiểm tra cookie và header Authorization
            context.Token = context.HttpContext.Request.Cookies["access_token"]
                            ?? context.HttpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("CorsPolicy");
app.MapControllers();
app.MapHub<ChatHub>("/chat");

app.Run();