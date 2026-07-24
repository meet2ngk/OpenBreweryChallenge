using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenBrewery.Core.Configuration;
using OpenBrewery.Core.Interfaces;
using OpenBrewery.Infrastructure.External.Clients;
using OpenBrewery.Infrastructure.Persistence.Context;
using OpenBrewery.Infrastructure.Persistence.Repositories;
using OpenBrewery.Infrastructure.Services;
using System.Text;

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

//Configuration (options)
builder.Services.Configure<OpenBreweryApiOptions>(builder.Configuration.GetSection("OpenBreweryApi"));
builder.Services.Configure<WebApiDataSourceOptions>(builder.Configuration.GetSection("WebApiDataSource"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? throw new InvalidOperationException("JWT configuration is missing.");

//DBContext
builder.Services.AddDbContext<BreweryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BreweryDatabase")));

//Repository
builder.Services.AddScoped<IBreweryRepository, BreweryRepository>();

//HTTP clients/ external API
builder.Services.AddHttpClient<IOpenBreweryClient, OpenBreweryClient>();

//Application Services
builder.Services.AddScoped<IOpenBreweryService, OpenBreweryService>();

//Cache
builder.Services.AddMemoryCache();

//Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            //signature
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

            //issuer
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            //audiance
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            //expiration
            ValidateLifetime = true,
        };
    });

//Swagger/ OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var connectionString =
    builder.Configuration.GetConnectionString("BreweryDatabase");

// HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();