using Shared.Contracts.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") 
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