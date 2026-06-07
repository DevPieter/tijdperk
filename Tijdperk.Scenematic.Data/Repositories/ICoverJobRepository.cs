using Tijdperk.Scenematic.Data.Models.DTOs;
using Tijdperk.Scenematic.Data.Models.Entities;

namespace Tijdperk.Scenematic.Data.Repositories;

public interface ICoverJobRepository
{
    Task<GenerateCoverJob> RegisterAsync(RegisterGenerateCoverDto dto);

    Task<GenerateCoverJob?> GetByIdAsync(Guid id);

    Task<bool> UpdateAsync(Guid id, UpdateGenerateCoverDto dto);
}