# Tijdperk

## Table of Contents

- [Project Structure](#project-strucure)
- [Local Development](#local-development)

## Project Strucure

Tijdperk is organized into the following projects:

- Tijdperk.Common.Core
- Tijdperk.Common.Data
- Tijdperk.Data.Postgres
- Tijdperk.Logic
- Tijdperk.Service.Bot

### Reference Flow

- `Tijdperk.Service.Bot`
    - -> `Tijdperk.Logic`
    - -> `Tijdperk.Data.Postgres`
- `Tijdperk.Logic` -> `Tijdperk.Common.Data`
- `Tijdperk.Data.Postgres` -> `Tijdperk.Common.Data`
- `Tijdperk.Common.Data` -> `Tijdperk.Common.Core`

## Local Development

### Prerequisites

- .NET 10 SDK
- PostgreSQL Database ([Setting Up PostgreSQL with Docker](#setting-up-postgresql-with-docker-recommended))
- Discord Bot Token

### Configuring App Settings

For local development, you can use the `appsettings.Development.json` file to store your local configuration. You need to create this file in the `Tijdperk.Service.Bot` project. An example configuration can be found in `appsettings.json`, which you can copy and modify as needed.

> [!WARNING]
> Do **NOT** commit your `appsettings.Development.json` file to git, as it **WILL contain sensitive information**.

### Setting Up PostgreSQL with Docker (Recommended)

Below is an example Docker run command to start a local PostgreSQL instance for development.

For this example, we will use the following configuration:

- Container Name: `tijdperk_postgres`
- Database Name: `tijdperk_postgres`
- Database User: `tijdperk_postgres`
- Database Password: **not set** (using trust authentication for simplicity in local development)

Connection String for the above configuration:

```
Server=localhost;Port=5432;Database=tijdperk_postgres;User Id=tijdperk_postgres;Password=;
```

#### Steps to Run PostgreSQL with Docker:

1. Make sure Docker is **installed** and **running** on your machine.
2. Run the following command in your terminal to start the PostgreSQL container:

``` bash
docker run --name tijdperk_postgres -e POSTGRES_USER=tijdperk_postgres -e POSTGRES_DB=tijdperk_postgres -e POSTGRES_HOST_AUTH_METHOD=trust -p 5432:5432 -d postgres
```

3. A PostgreSQL instance is now running on `localhost:5432` with the database name `tijdperk_postgres` and user `tijdperk_postgres`.
