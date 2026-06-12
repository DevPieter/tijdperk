# Tijdperk

## Table of Contents

- [Project Structure](#project-structure)
- [Local Development](#local-development)

## Project Structure

Tijdperk is organized into the following projects:

- Tijdperk.Common.Core
- Tijdperk.Common.Data
- Tijdperk.Data.Postgres
- Tijdperk.Logic
- Tijdperk.Scenematic.Bucket
- Tijdperk.Scenematic.Bucket.MinIO
- Tijdperk.Scenematic.Common
- Tijdperk.Scenematic.Data
- Tijdperk.Scenematic.Data.Postgres
- Tijdperk.Scenematic.Logic
- Tijdperk.Scenematic.Queue
- Tijdperk.Scenematic.Queue.Redis
- Tijdperk.Scenematic.Worker.CoverGenerator
- Tijdperk.Service.Bot
- Tijdperk.Service.Scenematic

### Reference Flow

- `Tijdperk.Service.Bot`
    - -> `Tijdperk.Logic`
    - -> `Tijdperk.Data.Postgres`

- `Tijdperk.Service.Scenematic`
    - -> `Tijdperk.Logic`
    - -> `Tijdperk.Data.Postgres`
    - -> `Tijdperk.Scenematic.Logic`
    - -> `Tijdperk.Scenematic.Data.Postgres`
    - -> `Tijdperk.Scenematic.Queue.Redis`

- `Tijdperk.Scenematic.Worker.CoverGenerator`
    - -> `Tijdperk.Scenematic.Logic`
    - -> `Tijdperk.Scenematic.Data.Postgres`
    - -> `Tijdperk.Scenematic.Queue.Redis`
    - -> `Tijdperk.Scenematic.Bucket.MinIO`

- `Tijdperk.Logic`
    - -> `Tijdperk.Common.Data`

- `Tijdperk.Data.Postgres`
    - -> `Tijdperk.Common.Data`

- `Tijdperk.Common.Data`
    - -> `Tijdperk.Common.Core`

- `Tijdperk.Scenematic.Logic`
    - -> `Tijdperk.Common.Core`
    - -> `Tijdperk.Scenematic.Bucket`
    - -> `Tijdperk.Scenematic.Data`
    - -> `Tijdperk.Scenematic.Queue`

- `Tijdperk.Scenematic.Bucket.MinIO`
    - -> `Tijdperk.Scenematic.Bucket`

- `Tijdperk.Scenematic.Bucket`
    - -> `Tijdperk.Common.Core`

- `Tijdperk.Scenematic.Data.Postgres`
    - -> `Tijdperk.Scenematic.Data`

- `Tijdperk.Scenematic.Data`
    - -> `Tijdperk.Common.Core`
    - -> `Tijdperk.Scenematic.Common`

- `Tijdperk.Scenematic.Queue.Redis`
    - -> `Tijdperk.Scenematic.Queue`

- `Tijdperk.Scenematic.Queue`
    - -> `Tijdperk.Scenematic.Common`

## Local Development

### Prerequisites

- .NET 10 SDK ([Download .NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0))
- Docker Desktop or Docker Engine with Docker Compose ([Getting Started with Docker](https://docs.docker.com/get-started/))
- Discord Bot Token ([How to obtain a token](#creating-a-discord-bot-and-obtaining-a-token))
- A guild/server where you have permissions to add the bot

- Local dependencies ([Setting Up Local Dependencies](#setting-up-local-dependencies))
    - **Two** PostgreSQL databases
    - A Redis instance
    - A MinIO instance

### Configuring App Settings

> [!WARNING]
> Do **NOT** commit your `appsettings.Development.json` file to git, as it **WILL contain sensitive information**.

> [!WARNING]
> Do **NOT** add sensitive information to `appsettings.json`. Always use `appsettings.Development.json` or environment variables for sensitive data.

For local development, create `appsettings.Development.json` files in the projects you want to run. Each project has an `appsettings.json` file that you can copy and modify as needed.

#### App Settings for `Tijdperk.Service.Bot`

| Setting                      | Description                                                                                                           |
|------------------------------|-----------------------------------------------------------------------------------------------------------------------|
| `Discord.Bot.Token`          | Your Discord Bot Token. [How to obtain a token](#creating-a-discord-bot-and-obtaining-a-token)                        |
| `Discord.Bot.CustomStatus`   | (Optional) A custom status message for your bot.                                                                      |
| `Guild.Id`                   | The ID of the guild/server where the bot will operate. [Obtaining Guild IDs](#obtaining-guild-ids)                    |
| `Guild.MusicChannelId`       | The ID of the channel where the bot will send music-related messages. [Obtaining Channel IDs](#obtaining-channel-ids) |
| `ConnectionStrings.Postgres` | Main Tijdperk PostgreSQL connection string.                                                                           |

Example:

```json
{
  "ConnectionStrings": {
    "Postgres": "Server=localhost;Port=5432;Database=tijdperk;User Id=tijdperk;Password=tijdperk;"
  },
  "Discord": {
    "Bot": {
      "Token": "YOUR_DISCORD_BOT_TOKEN",
      "CustomStatus": ""
    },
    "Guild": {
      "Id": 123456789012345678,
      "MusicChannelId": 123456789012345678
    }
  }
}
```

#### App Settings for `Tijdperk.Service.Scenematic`

| Setting                        | Description                                                        |
|--------------------------------|--------------------------------------------------------------------|
| `ConnectionStrings.Tijdperk`   | Main Tijdperk PostgreSQL connection string.                        |
| `ConnectionStrings.Scenematic` | Scenematic PostgreSQL connection string.                           |
| `Scenematic.Redis.Host`        | Redis host.                                                        |
| `Scenematic.Redis.Port`        | Redis port.                                                        |
| `Scenematic.Redis.Password`    | Redis password. *Use an empty string for the local compose setup.* |

Example:

```json
{
  "ConnectionStrings": {
    "Tijdperk": "Server=localhost;Port=5432;Database=tijdperk;User Id=tijdperk;Password=tijdperk;",
    "Scenematic": "Server=localhost;Port=5433;Database=tijdperk_scenematic;User Id=tijdperk_scenematic;Password=tijdperk_scenematic;"
  },
  "Scenematic": {
    "Redis": {
      "Host": "localhost",
      "Port": 6379,
      "Password": ""
    }
  }
}
```

#### App Settings for `Tijdperk.Scenematic.Worker.CoverGenerator`

| Setting                                      | Description                                                                        |
|----------------------------------------------|------------------------------------------------------------------------------------|
| `ConnectionStrings.Scenematic`               | Scenematic PostgreSQL connection string.                                           |
| `Scenematic.Redis.Host`                      | Redis host.                                                                        |
| `Scenematic.Redis.Port`                      | Redis port.                                                                        |
| `Scenematic.Redis.Password`                  | Redis password. *Use an empty string for the local compose setup.*                 |
| `Scenematic.MinIO.Endpoint`                  | MinIO endpoint used by the worker.                                                 |
| `Scenematic.MinIO.AccessKey`                 | MinIO access key.                                                                  |
| `Scenematic.MinIO.SecretKey`                 | MinIO secret key.                                                                  |
| `Scenematic.MinIO.UseSSL`                    | Whether to use SSL when connecting to MinIO. Set to `false` for local development. |
| `Scenematic.Bucket.Cover.Bucket`             | Bucket name for generated covers.                                                  |
| `Scenematic.Bucket.Cover.ObjectNameTemplate` | Object name template for generated cover files.                                    |

Example:

```json
{
  "ConnectionStrings": {
    "Scenematic": "Server=localhost;Port=5433;Database=tijdperk_scenematic;User Id=tijdperk_scenematic;Password=tijdperk_scenematic;"
  },
  "Scenematic": {
    "Redis": {
      "Host": "localhost",
      "Port": 6379,
      "Password": ""
    },
    "MinIO": {
      "Endpoint": "localhost:9000",
      "AccessKey": "YOUR_MINIO_ACCESS_KEY",
      "SecretKey": "YOUR_MINIO_SECRET_KEY",
      "UseSSL": false
    },
    "Bucket": {
      "Cover": {
        "Bucket": "tijdperk",
        "ObjectNameTemplate": "cover/{Group}/cover-{Id}.{Extension}"
      }
    }
  }
}
```

# Tutorials

## Setting Up Local Dependencies

The local development environment needs:

- PostgreSQL for the main Tijdperk data model
- PostgreSQL for the Scenematic data model
- Redis for Scenematic queue jobs
- MinIO for generated cover storage

Start the local dependencies:

```bash
docker compose -f compose.local.yaml up -d
```

Stop the local dependencies:

```bash
docker compose -f compose.local.yaml down
```

Remove the local containers and volumes:

```bash
docker compose -f compose.local.yaml down -v
```

The local connection details are:

| Dependency            | Local address    | Credentials                                                                                   |
|-----------------------|------------------|-----------------------------------------------------------------------------------------------|
| Main PostgreSQL       | `localhost:5432` | Database: `tijdperk`, user: `tijdperk`, password: `tijdperk`                                  |
| Scenematic PostgreSQL | `localhost:5433` | Database: `tijdperk_scenematic`, user: `tijdperk_scenematic`, password: `tijdperk_scenematic` |
| Redis                 | `localhost:6379` | No password                                                                                   |
| MinIO API             | `localhost:9000` | Root user: `tijdperk_minio`, root password: `tijdperk_minio_password`                         |

### Setting Up MinIO with the CLI Client

Use the MinIO CLI Client to create the local bucket and configure public read access for generated cover objects.

1. Download the latest Windows MinIO CLI Client from [MinIO Client releases](https://github.com/minio/mc/releases/latest).
2. Download the `mc.windows-[...].exe` file, for example `mc.windows-amd64.RELEASE.2025-08-13T08-35-41Z.exe`.
3. Rename the downloaded file to `mc.exe`.
4. You can either add the folder containing `mc.exe` to your system `PATH` or run it directly from the download location.

Check that the client works:

```bash
mc --version
```

If `mc.exe` is not on your `PATH`, run the same commands with `.\mc.exe` from the download folder.

Create an alias for the local MinIO instance:

```bash
mc alias set tijdperk-local http://localhost:9000 tijdperk_minio tijdperk_minio_password
```

Create the bucket used by `Tijdperk.Scenematic.Worker.CoverGenerator`:

```bash
mc mb tijdperk-local/tijdperk
```

Create a file named `minio-public-read.json` with the following policy (location does not matter):

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "AWS": [
          "*"
        ]
      },
      "Action": [
        "s3:GetObject"
      ],
      "Resource": [
        "arn:aws:s3:::tijdperk/*"
      ]
    }
  ]
}
```

Apply the anonymous read policy to the bucket:

```bash
mc anonymous set-json "C:\path\to\minio-public-read.json" tijdperk-local/tijdperk
```

Create an access key for the application:

```bash
mc admin accesskey create tijdperk-local/ --expiry-duration 0
```

The command prints an `Access Key` and `Secret Key`. Use those values in `Tijdperk.Scenematic.Worker.CoverGenerator/appsettings.Development.json`:

```json
{
  "Scenematic": {
    "MinIO": {
      "Endpoint": "localhost:9000",
      "AccessKey": "ACCESS_KEY_FROM_MC",
      "SecretKey": "SECRET_KEY_FROM_MC",
      "UseSSL": false
    }
  }
}
```

## Running Local Development

Start the local dependencies first:

```bash
docker compose -f compose.local.yaml up -d
```

Run the Bot service:

```bash
dotnet run --project Tijdperk.Service.Bot/Tijdperk.Service.Bot.csproj
```

Run the Scenematic API:

```bash
dotnet run --project Tijdperk.Service.Scenematic/Tijdperk.Service.Scenematic.csproj
```

Run the Scenematic CoverGenerator worker:

```bash
dotnet run --project Tijdperk.Scenematic.Worker.CoverGenerator/Tijdperk.Scenematic.Worker.CoverGenerator.csproj
```

## Creating a Discord Bot and Obtaining a Token

> [!NOTE]
> This section needs to be added to the documentation. Sorry for the inconvenience.

TODO - Add instructions on how to create a Discord bot and obtain a token.

## Enabling Developer Mode in Discord

1. Open Discord and go to **User Settings** (the gear icon at the bottom left).
2. Search for `developer` in the settings search bar.
3. Enable **Developer Mode** by toggling the switch.

## Obtaining Guild IDs

1. Make sure you have **Developer Mode enabled** in Discord ([see above](#enabling-developer-mode-in-discord)).
2. Right-click on the guild/server icon.
3. Select **Copy ID** from the context menu. This will copy the guild ID to your clipboard.

## Obtaining Channel IDs

1. Make sure you have **Developer Mode enabled** in Discord ([see above](#enabling-developer-mode-in-discord)).
2. Right-click on the channel name.
3. Select **Copy ID** from the context menu. This will copy the channel ID to your clipboard.
