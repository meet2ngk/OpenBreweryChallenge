# Open Brewery API

A .NET 8 Web API built using the Open Brewery DB API with support for external API and local SQLite database data sources.

## Features

- .NET 8 / ASP.NET Core Web API
- Open Brewery DB API integration
- SQLite database with Entity Framework Core
- Configurable data source:
  - External API
  - Local SQLite database
- Pagination
- Search breweries by:
  - Name
  - City
- Sorting by:
  - Name
  - City
  - Distance
- Ascending and descending sorting
- Distance calculation using latitude and longitude
- In-memory caching
- API versioning
- JWT authentication and authorization
- Swagger / OpenAPI documentation
- Centralized error handling and logging
- Database initialization and resumable data synchronization
- Unit tests using xUnit and Moq

## Technologies

- C#
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger / OpenAPI
- JWT Authentication
- xUnit
- Moq

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or Visual Studio Code

### Clone the Repository

```bash
git clone https://github.com/meet2ngk/OpenBreweryChallenge.git
cd OpenBreweryChallenge