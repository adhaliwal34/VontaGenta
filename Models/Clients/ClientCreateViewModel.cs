#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace VontaGenta.Models
{
    public class ClientCreateViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Display(Name = "Creator")]
        public string UserName { get; set; }
    }
}
