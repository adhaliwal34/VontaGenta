using VontaGenta.Entities;

namespace VontaGenta.Data
{
    public interface IAppTimeZonesRepo
    {
        Task<List<AppTimeZone>> GetTimeZones(CancellationToken ct);
    }
}