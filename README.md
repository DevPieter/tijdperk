# Tijdperk

## Project Strucure

- Tijdperk.Common.Core
- Tijdperk.Common.Data
- Tijdperk.Data.Postgres
- Tijdperk.Logic
- Tijdperk.Service.Bot

### Reference Flow

- `Tijdperk.Service.Bot`
- - -> `Tijdperk.Logic`
- - -> `Tijdperk.Data.Postgres`

- `Tijdperk.Logic` -> `Tijdperk.Common.Data`
- `Tijdperk.Data.Postgres` -> `Tijdperk.Common.Data`
- `Tijdperk.Common.Data` -> `Tijdperk.Common.Core`