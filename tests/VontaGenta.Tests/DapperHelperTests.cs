using VontaGenta.Entities;
using VontaGenta.Helpers;

namespace VontaGenta.Tests
{
    public class DapperHelperTests
    {
        [Fact]
        public void Test1()
        {
            var orgs = new List<Org>{
                new Org { Name = "Alice", ID = 1 },
                new Org { Name = "Bob", ID = 2 },
                new Org { Name = "Charles", ID = 3 },
            };

            var colNames = new List<string> { "Name", "ID" };

            var dapperHelper = new DapperHelper();
            var sql = dapperHelper.PrepareBulkInsertStatement("Users", colNames, orgs.Count);

            Assert.NotNull(sql);
        }
    }
}
