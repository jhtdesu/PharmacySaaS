using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://jhtdesu-app.tech") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

builder.Services.AddSharedExceptionHandling();

var app = builder.Build();

app.UseExceptionHandler();
app.UseRouting();
app.UseCors("AllowFrontend"); 
app.MapReverseProxy();

app.Run();