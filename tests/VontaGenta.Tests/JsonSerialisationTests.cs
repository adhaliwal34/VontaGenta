using Newtonsoft.Json;
using NodaTime;
using VontaGenta.Entities;

namespace VontaGenta.Tests
{
    public class JsonSerialisationTests
    {

        [Fact]
        public void Test_serialisation_of_dates_and_ignore_objects()
        {
            var appUser = new AppUser { Id = "ldhaf-dladj-aldj", FirstNames = "AJ", LastName = "Dhaliwal" };
            var org = new Org { ID = 1, Name = "test org", Description = "testing serialisation", AppUserID = appUser.Id, User = appUser };
            var client = new Client { ID = 3, Name = "test client", Description = "testing serialisation", AppUserID = appUser.Id, OrgID = org.ID, Org = org, User = appUser };

            // Testing UTC format date time
            var local = LocalDateTime.FromDateTime(DateTime.Now);
            var zone = DateTimeZoneProviders.Tzdb["Asia/Kuala_Lumpur"];
            ZonedDateTime zonedTime = local.InZoneLeniently(zone);
            // I don't want .NET to add Z letters to the DateTimes when saving to the database so DateTimeKind must be unspecified.
            var startDate = DateTime.SpecifyKind(zonedTime.ToDateTimeUtc(), DateTimeKind.Unspecified);

            var project = new Project { ID = 123, Name = "Important project 😊", Description = "testing serialisation", StartDate = startDate, EndDate = startDate.AddDays(30), AppUserID = appUser.Id, User = appUser, ClientID = client.ID, Client = client };

            string output = JsonConvert.SerializeObject(project);

            Assert.NotNull(output);
        }
    }
}
