using System.Text;
using MessengerApplication.Hubs;
using MessengerApplication.Models;
using MessengerApplication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var origins = "*";
// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
        if (origins is not { Length: > 0 }) return;

        if (origins.Contains("*"))
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
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MessengerApplicationDatabaseSettings>(
    builder.Configuration.GetSection("MessengerApplicationDatabase"));
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<DatabaseProviderService>();
builder.Services.AddScoped<ChatsService>();
builder.Services.AddScoped<MessagesService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<ChatHub>();
builder.Services.AddRouting();
builder.Services.AddMemoryCache();
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