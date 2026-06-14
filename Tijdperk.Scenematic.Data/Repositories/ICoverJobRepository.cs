using Tijdperk.Scenematic.Data.Models.DTOs.Covers;
using Tijdperk.Scenematic.Data.Models.Entities.Covers;

namespace Tijdperk.Scenematic.Data.Repositories;

public interface ICoverJobRepository
{
    Task<GenerateCoverJob> RegisterAsync(RegisterGenerateCoverDto dto);

    Task<GenerateCoverJob?> GetByIdAsync(
        Guid id,
        bool includeMetadata = false,
        bool includeResults = false
    );

    Task<GenerateCoverJob?> GetByMetaHashAsync(
        string metaHash,
        bool includeMetadata = false,
        bool includeResults = false
    );

    Task<bool> UpdateAsync(Guid id, UpdateGenerateCoverDto dto);

    Task<bool> HasSuccessAsync(Guid id);
    Task<GenerateCoverJobSuccessResult?> SetSuccessAsync(Guid id, SetGenerateCoverJobSuccessDto dto);

    Task<bool> HasFailureAsync(Guid id);
    Task<GenerateCoverJobFailureResult?> SetFailureAsync(Guid id, SetGenerateCoverJobFailureDto dto);

    Task<bool> DeleteMetaAsync(Guid id);
}