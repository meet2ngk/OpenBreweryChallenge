using OpenBrewery.Core.Configuration;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Infrastructure.External.Clients;
using OpenBrewery.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<OpenBreweryApiOptions>(builder.Configuration.GetSection("OpenBreweryApi"));
builder.Services.AddHttpClient<IOpenBreweryClient, OpenBreweryClient>();

builder.Services.AddScoped<IOpenBreweryService, OpenBreweryService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
