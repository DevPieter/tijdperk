using Microsoft.EntityFrameworkCore;
using Tijdperk.Common.Data.Models.Entities.SharedSongs;
using Tijdperk.Common.Data.Models.Entities.Songs;
using Tijdperk.Common.Data.Models.Entities.SongVotes;

namespace Tijdperk.Data.Postgres.Data;

public class NpgsqlDataContext(DbContextOptions<NpgsqlDataContext> options) : DbContext(options)
{
    public DbSet<SongEntity> Songs { get; set; }
    public DbSet<SharedSongEntity> SharedSongs { get; set; }
    public DbSet<SongVoteEntity> SongVotes { get; set; }
}