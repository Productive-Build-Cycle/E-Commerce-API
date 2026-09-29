using ECommerce.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddInfrastructure(
    builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
