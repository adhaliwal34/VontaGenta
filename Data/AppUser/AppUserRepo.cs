using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using VontaGenta.Entities;
using VontaGenta.Helpers;

namespace VontaGenta.Data
{
    public class AppUserRepo : ErrorHelpers, IAppUserRepo
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AppUserRepo> _logger;

        public AppUserRepo(UserManager<AppUser> userManager,
            IConfiguration config, ILogger<AppUserRepo> logger)
        {
            _userManager = userManager;
            _config = config;
            _logger = logger;
        }

        public async Task<AppUser?> GetAppUser(ClaimsPrincipal user)
        {
            return await _userManager.GetUserAsync(user);
        }

        public async Task<AppUser?> GetAppUserAndRelatedFields(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            try
            {
                email = email.ToUpper();
                var connectionString = _config["ConnectionStrings:ApplicationDbContext"];
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    var parameters = new { Email = email };
                    var sql = @"SELECT 
                            aspnetusers.*
                            ,apptimezones.*
                            FROM aspnetusers
                            INNER JOIN apptimezones ON apptimezones.id = aspnetusers.timezoneid
                            WHERE aspnetusers.normalizedusername = @Email;";
                    return (await connection.QueryAsync<AppUser, AppTimeZone, AppUser>(sql, (appUser, appTimeZone) =>
                    {
                        appUser.TimeZone = appTimeZone;
                        return appUser;
                    }, parameters)).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }
    }
}
