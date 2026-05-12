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
- PostgreSQL Database ([Setting Up PostgreSQL with Docker](#setting-up-postgresql-with-docker))
- Discord Bot Token ([How to obtain a token](#creating-a-discord-bot-and-obtaining-a-token))
- A guild/server where you have permissions to add the bot

### Configuring App Settings

> [!WARNING]
> Do **NOT** commit your `appsettings.Development.json` file to git, as it **WILL contain sensitive information**.

> [!WARNING]
> Do **NOT** add sensitive information to `appsettings.json`. Always use `appsettings.Development.json` or environment variables for sensitive data.

For local development, you can use the `appsettings.Development.json` file to store your local configuration. You need to create this file in the `Tijdperk.Service.Bot` project. An example configuration can be found in `appsettings.json`, which you can copy and modify as needed.

#### App Settings for `Tijdperk.Service.Bot`

| Setting                    | Description                                                                                                           |
| -------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| `Discord.Bot.Token`        | Your Discord Bot Token. [How to obtain a token](#creating-a-discord-bot-and-obtaining-a-token)                        |
| `Discord.Bot.CustomStatus` | (Optional) A custom status message for your bot.                                                                      |
| `Guild.Id`                 | The ID of the guild/server where the bot will operate. [Obtaining Guild IDs](#obtaining-guild-ids)                    |
| `Guild.MusicChannelId`     | The ID of the channel where the bot will send music-related messages. [Obtaining Channel IDs](#obtaining-channel-ids) |

# Tutorials

## Setting Up PostgreSQL with Docker

Below is an example **Docker run command** to start a local PostgreSQL instance for development.

For this example, we will use the following configuration:

- Database Name: `tijdperk_postgres`
- Database User: `tijdperk_postgres`
- Database Password: **not set** (using trust authentication for simplicity in local development)

Connection String for the above configuration:

```
Server=localhost;Port=5432;Database=tijdperk_postgres;User Id=tijdperk_postgres;Password=;
```

### Steps to Run PostgreSQL with Docker:

1. Make sure Docker is **installed** and **running** on your machine.
2. Run the following command in your terminal to start the PostgreSQL container:

``` bash
docker run --name tijdperk_postgres -e POSTGRES_USER=tijdperk_postgres -e POSTGRES_DB=tijdperk_postgres -e POSTGRES_HOST_AUTH_METHOD=trust -p 5432:5432 -d postgres
```

3. A PostgreSQL instance is now running on `localhost:5432` with the database name `tijdperk_postgres` and user `tijdperk_postgres`.

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
