using Asp.Versioning;
using OpenBrewery.Core.Configuration;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Infrastructure.External.Clients;
using OpenBrewery.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

//Controllers
builder.Services.AddControllers();

//Api-versioning
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

//Swagger/ OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Open Brewery API",
        Version = "v1"
    });
    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Open Brewery API",
        Version = "v2"
    });
});

//Configuration (options)
builder.Services.Configure<OpenBreweryApiOptions>(builder.Configuration.GetSection("OpenBreweryApi"));

//HTTP client/ external API
builder.Services.AddHttpClient<IOpenBreweryClient, OpenBreweryClient>();

//Services
builder.Services.AddScoped<IOpenBreweryService, OpenBreweryService>();

//Cache
builder.Services.AddMemoryCache();

var app = builder.Build();

// HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Open Brewery API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Open Brewery API v2");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();