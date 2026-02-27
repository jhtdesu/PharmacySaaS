using Inventory.Infrastructure;
using Inventory.Application;
using Inventory.Application.Common.Interfaces;
using Inventory.Api.Services;
using Inventory.Api.Middlewares;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


var app = builder.Build();


// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler();
app.MapControllers();

app.Run();