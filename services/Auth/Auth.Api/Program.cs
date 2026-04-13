using System.Text;
using Auth.Api.Data;
using Auth.Api.Models;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RabbitMQ.Client;
using Shared.Contracts.ExceptionHandling;
using Shared.Contracts.Configuration;


var builder = WebApplication.CreateBuilder(args);

var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
connectionString = connectionString
    .Replace("PGUSER", Environment.GetEnvironmentVariable("PGUSER") ?? "")
    .Replace("PGPASSWORD", Environment.GetEnvironmentVariable("PGPASSWORD") ?? "");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

// RabbitMQ Connection
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ");
var rabbitMqHost = rabbitMqSettings["Host"] ?? "rabbitmq";
var rabbitMqPort = int.TryParse(rabbitMqSettings["Port"], out var port) ? port : 5672;
var rabbitMqUser = rabbitMqSettings["User"] ?? "guest";
var rabbitMqPassword = rabbitMqSettings["Password"] ?? "guest";

var connectionFactory = new ConnectionFactory
{
    HostName = rabbitMqHost,
    Port = rabbitMqPort,
    UserName = rabbitMqUser,
    Password = rabbitMqPassword,
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
};

builder.Services.AddSingleton<IConnectionFactory>(connectionFactory);

builder.Services.AddScoped<IMessageQueueService, RabbitMqMessageQueueService>();
builder.Services.AddHostedService<MomoPaymentWorkerService>();

builder.Services.AddControllers();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddHttpClient();
builder.Services.Configure<MomoOptions>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGci...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

builder.Services.AddSharedExceptionHandling();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// JWT
app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();
app.MapControllers();

app.Run();