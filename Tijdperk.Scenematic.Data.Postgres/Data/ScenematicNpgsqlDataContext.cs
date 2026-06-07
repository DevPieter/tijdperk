using Microsoft.EntityFrameworkCore;
using Tijdperk.Scenematic.Data.Models.Entities;

namespace Tijdperk.Scenematic.Data.Postgres.Data;

public class ScenematicNpgsqlDataContext(DbContextOptions<ScenematicNpgsqlDataContext> options) : DbContext(options)
{
    public DbSet<GenerateCoverJob> GenerateCoverJobs { get; set; } = null!;
}