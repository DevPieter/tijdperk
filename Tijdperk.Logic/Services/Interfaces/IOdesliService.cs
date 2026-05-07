using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Logic.Models.Odesli;

namespace Tijdperk.Logic.Services.Interfaces;

public interface IOdesliService
{
    Task<ResultObject<OdesliResponse>> GetSongInfoAsync(string songUrl);
}