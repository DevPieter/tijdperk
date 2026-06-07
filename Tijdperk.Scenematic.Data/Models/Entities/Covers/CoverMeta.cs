using System.ComponentModel.DataAnnotations;

namespace Tijdperk.Scenematic.Data.Models.Entities.Covers;

public class CoverMeta
{
    public Guid Id { get; init; }

    [Required]
    public string AlbumTitle { get; init; } = null!;

    [Required]
    public string AlbumArtist { get; init; } = null!;

    [Required]
    public string AlbumCoverImageUrl { get; init; } = null!;

    public ICollection<CoverVoteMeta> CoverVotes { get; set; } = new List<CoverVoteMeta>();
}