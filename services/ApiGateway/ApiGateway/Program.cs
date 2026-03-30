using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://54.253.146.69:3000", "http://54.253.146.69:5173") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

builder.Services.AddSharedExceptionHandling();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("AllowFrontend"); 
app.MapReverseProxy();

app.Run();