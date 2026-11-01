using VontaGenta.AppEnums;
using VontaGenta.ViewHelpers;

namespace VontaGenta.Tests
{
    public class EnumTests
    {
        // It should not return 1 and 2 as would be done if bit anding was used.
        [Fact]
        public void Filter_enum_does_not_use_flags()
        {
            var selectListHelpers = new SelectListHelpers();
            var thingsFilter = new List<Things> { Things.Three };
            var list = selectListHelpers.GetFilteredEnumSelectList(thingsFilter);

            Assert.NotNull(list);
            Assert.Single(list);
            Assert.Contains(list, l => l.Value == "3");
        }

        [Fact]
        public void Filter_enum_adds_zero()
        {
            var selectListHelpers = new SelectListHelpers();
            var thingsFilter = new List<Things> { Things.None, Things.Three };
            var list = selectListHelpers.GetFilteredEnumSelectList(thingsFilter);

            Assert.NotNull(list);
            Assert.Equal(2, list.Count());
            Assert.Contains(list, l => l.Value == "0");
            Assert.Contains(list, l => l.Value == "3");
        }

        [Fact]
        public void Filter_enum_adds_only_zero()
        {
            var selectListHelpers = new SelectListHelpers();
            var thingsFilter = new List<Things> { Things.None };
            var list = selectListHelpers.GetFilteredEnumSelectList(thingsFilter);

            Assert.NotNull(list);
            Assert.Single(list);
            Assert.Contains(list, l => l.Value == "0");
        }

        [Fact]
        public void Filter_enum_adds_each_given_member()
        {
            var selectListHelpers = new SelectListHelpers();
            var thingsFilter = new List<Things> { Things.None, Things.One, Things.Two, Things.Three };
            var list = selectListHelpers.GetFilteredEnumSelectList(thingsFilter);

            Assert.NotNull(list);
            Assert.Equal(4, list.Count());
            Assert.Contains(list, l => l.Value == "0");
            Assert.Contains(list, l => l.Value == "1");
            Assert.Contains(list, l => l.Value == "2");
            Assert.Contains(list, l => l.Value == "3");
        }
    }
}
