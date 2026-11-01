using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Entities;

namespace VontaGenta.Models
{
    public interface IHasProjectsSelectLists
    {
        public List<Project> ProjectList { get; set; }
        SelectList Projects { get; set; }
    }
}
