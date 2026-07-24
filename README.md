# OpenBreweryChallenge
OpenBrewery Challenge repository - e.l.f. technical assingnment


# Open Brewery API

A .NET 8 ASP.NET Core Web API that integrates with the Open Brewery API to retrieve and manage brewery information.

## Features

- ASP.NET Core Web API with API versioning
- JWT authentication and authorization
- Swagger/OpenAPI
- External API integration using typed `HttpClient`
- Search breweries by name or city
- Sort by name, city, or distance
- Latitude/longitude based distance calculation
- Request validation using Data Annotations and custom validation
- In-memory caching using `IMemoryCache`
- SQLite database with Entity Framework Core
- Repository pattern
- Configurable data source (SQLite or External API)
- Initial database population from the external API when the SQLite database is empty

## Setup

1. Clone the repository and open the solution in Visual Studio.
2. Restore NuGet packages.
3. Run the following command from the Package Manager Console to apply the existing EF Core migrations:

```powershell
Update-Database -Project OpenBrewery.Infrastructure -StartupProject OpenBrewery.Api