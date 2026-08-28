using EnumsNET;
using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Models;
using System.ComponentModel.DataAnnotations;

namespace VontaGenta.ViewHelpers
{
    // TODO: Move code to view model builder, which will populate view model for controllers and api calls.
    // view model builder will receive injected services.
    // controller will do steps necessary for web pages only.
    public class SelectListHelpers
    {

        public string? SetCategoriesSelectList(IHasCategoriesSelectLists vm)
        {
            try
            {
                vm.Categories = new SelectList(vm.CategoryList, "ID", "Name");
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        public string? SetProjectSelectList(IHasProjectsSelectLists vm)
        {
            try
            {
                vm.Projects = new SelectList(vm.ProjectList, "ID", "Name");
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        public string? SetCurrenciesSelectList(IHasCurrenciesSelectLists vm)
        {
            try
            {
                vm.Currencies = new SelectList(vm.CurrencyList, "ID", "LongName");
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        public string? SetClientSelectList(IHasClientsSelectLists vm)
        {
            try
            {
                vm.Clients = new SelectList(vm.ClientList, "ID", "Name");
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        public SelectList GetEnumSelectList<T>(object? selectedValue = null, bool? isNullable = null) where T : Enum
        {

            var items = new List<SelectListItem>();

            if (isNullable == true)
            {
                items.Add(new SelectListItem { Text = "n/a", Value = null });
            }

            items.AddRange(Enums.GetMembers(typeof(T))
                             .Select(m => new SelectListItem
                             {
                                 Text = m.Attributes.Get<DisplayAttribute>()?.Name ?? m.Name,
                                 Value = ((int)m.Value).ToString()
                             })
                             .ToList());

            var selectList = new SelectList(items, "Value", "Text", selectedValue);

            SetNullValue(selectedValue, isNullable, selectList);

            return selectList;
        }

        public SelectList GetFilteredEnumSelectList<T>(List<T> values, object? selectedValue = null, bool? isNullable = null) where T : Enum
        {

            var items = new List<SelectListItem>();

            if (isNullable == true)
            {
                items.Add(new SelectListItem { Text = "n/a", Value = null });
            }

            var members = Enums.GetMembers(typeof(T));

            var membersFiltered = new List<EnumMember>();

            for (int i = 0; i < members.Count; i++)
            {
                for (int j = 0; j < values.Count; j++)
                {
                    if (((T)members[i].Value).Equals(values[j]))
                    {
                        membersFiltered.Add(members[i]);
                        break;
                    }
                }
            }

            items.AddRange(membersFiltered
                             .Select(m => new SelectListItem
                             {
                                 Text = m.Attributes.Get<DisplayAttribute>()?.Name ?? m.Name,
                                 Value = ((int)m.Value).ToString()
                             })
                             .ToList());

            var selectList = new SelectList(items, "Value", "Text", selectedValue);

            SetNullValue(selectedValue, isNullable, selectList);

            return selectList;
        }

        private void SetNullValue(object? selectedValue, bool? isNullable, SelectList selectList)
        {
            if (selectedValue == null && isNullable == true)
            {
                var nullOption = selectList.FirstOrDefault(x => x.Value == string.Empty);
                if (nullOption != null)
                {
                    nullOption.Selected = true;
                }
            }
        }
    }
}
