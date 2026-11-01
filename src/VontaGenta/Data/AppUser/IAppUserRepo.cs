using System.Security.Claims;
using VontaGenta.Entities;

namespace VontaGenta.Data
{
    public interface IAppUserRepo
    {
        Task<AppUser?> GetAppUser(ClaimsPrincipal user);
        Task<AppUser?> GetAppUserAndRelatedFields(string? userId);
    }
}
