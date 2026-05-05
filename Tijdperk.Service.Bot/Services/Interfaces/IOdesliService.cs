using Tijdperk.Service.Bot.Models.Odesli;

namespace Tijdperk.Service.Bot.Services.Interfaces;

public interface IOdesliService
{
    Task<OdesliResponse?> GetOdesliAsync(string songUrl);
}