using Microsoft.EntityFrameworkCore;
using Tijdperk.Common.Data.Models.Entities.SharedSongs;
using Tijdperk.Common.Data.Models.Entities.Songs;
using Tijdperk.Common.Data.Models.Entities.SongVotes;

namespace Tijdperk.Data.Postgres.Data;

public class NpgsqlDataContext(DbContextOptions<NpgsqlDataContext> options) : DbContext(options)
{
    public DbSet<SongEntity> Songs { get; set; }
    public DbSet<PlatformLinkEntity> PlatformLinks { get; set; }
    public DbSet<SharedSongEntity> SharedSongs { get; set; }
    public DbSet<SongVoteEntity> SongVotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SongEntity>()
            .HasMany(s => s.PlatformLinks)
            .WithOne(pl => pl.Song)
            .HasForeignKey(pl => pl.SongId);

        modelBuilder.Entity<SongEntity>()
            .HasMany(s => s.SharedSongs)
            .WithOne(ss => ss.Song)
            .HasForeignKey(ss => ss.SongId);

        modelBuilder.Entity<SongEntity>()
            .HasMany(s => s.SongVotes)
            .WithOne(sv => sv.Song)
            .HasForeignKey(sv => sv.SongId);
    }
}