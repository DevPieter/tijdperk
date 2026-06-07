using Microsoft.EntityFrameworkCore;
using Tijdperk.Scenematic.Data.Models.Entities.Covers;

namespace Tijdperk.Scenematic.Data.Postgres.Data;

public class ScenematicNpgsqlDataContext(DbContextOptions<ScenematicNpgsqlDataContext> options) : DbContext(options)
{
    public DbSet<GenerateCoverJob> GenerateCoverJobs { get; set; } = null!;

    public DbSet<CoverMeta> CoverMetas { get; set; } = null!;
    public DbSet<CoverVoteMeta> CoverVoteMetas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenerateCoverJob>()
            .HasOne(j => j.CoverMeta)
            .WithOne()
            .HasForeignKey<GenerateCoverJob>(j => j.CoverMetaId);

        modelBuilder.Entity<CoverVoteMeta>()
            .HasOne(j => j.CoverMeta)
            .WithMany(cm => cm.CoverVotes)
            .HasForeignKey(j => j.CoverMetaId);
    }
}