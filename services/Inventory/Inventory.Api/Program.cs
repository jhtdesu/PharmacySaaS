using Inventory.Infrastructure;
using Inventory.Application;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(connectionString, b => 
        b.MigrationsAssembly("Inventory.Infrastructure"))); 

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();


// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();