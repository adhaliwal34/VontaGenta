using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Entities;

namespace VontaGenta.Models
{
    public interface IHasCategoriesSelectLists
    {
        public List<Category> CategoryList { get; set; }
        SelectList Categories { get; set; }
    }
}
