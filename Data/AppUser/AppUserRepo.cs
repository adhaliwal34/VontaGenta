using Microsoft.AspNetCore.Identity;
using VontaGenta.Entities;
using MySqlConnector;
using System.Security.Claims;
using Dapper;
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
                using (var connection = new MySqlConnection(connectionString))
                {
                    var parameters = new { Email = email };
                    var sql = @"SELECT 
                            AspNetUsers.*
                            ,AppTimeZones.*
                            FROM AspNetUsers
                            INNER JOIN AppTimeZones ON AppTimeZones.ID = AspNetUsers.TimeZoneId
                            WHERE AspNetUsers.NormalizedUserName = @Email;";
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
