using Microsoft.EntityFrameworkCore;
using Tijdperk.Scenematic.Data.Models.Entities.Covers;

namespace Tijdperk.Scenematic.Data.Postgres.Data;

public class ScenematicNpgsqlDataContext(DbContextOptions<ScenematicNpgsqlDataContext> options) : DbContext(options)
{
    public DbSet<GenerateCoverJob> GenerateCoverJobs { get; set; } = null!;
    public DbSet<GenerateCoverJobSuccessResult> GenerateCoverJobSuccessResults { get; set; } = null!;
    public DbSet<GenerateCoverJobFailureResult> GenerateCoverJobFailureResults { get; set; } = null!;

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

        modelBuilder.Entity<GenerateCoverJobSuccessResult>()
            .HasKey(sa => sa.JobId);

        modelBuilder.Entity<GenerateCoverJobSuccessResult>()
            .Property(sa => sa.JobId)
            .ValueGeneratedNever();

        modelBuilder.Entity<GenerateCoverJobFailureResult>()
            .HasKey(fa => fa.JobId);

        modelBuilder.Entity<GenerateCoverJobFailureResult>()
            .Property(fa => fa.JobId)
            .ValueGeneratedNever();

        modelBuilder.Entity<GenerateCoverJob>()
            .HasOne(j => j.SuccessResult)
            .WithOne()
            .HasForeignKey<GenerateCoverJobSuccessResult>(sa => sa.JobId);

        modelBuilder.Entity<GenerateCoverJob>()
            .HasOne(j => j.FailureResult)
            .WithOne()
            .HasForeignKey<GenerateCoverJobFailureResult>(fa => fa.JobId);
    }
}