#nullable disable
using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Entities;
using System.ComponentModel.DataAnnotations;
namespace VontaGenta.Models
{
    public class ProjectCreateViewModel : IHasClientsSelectLists
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Display(Name = "Creator")]
        public string UserName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        [Display(Name = "Client")]
        public int ClientID { get; set; }
        public SelectList Clients { get; set; }
        public List<Client> ClientList { get; set; }

        public ProjectCreateViewModel()
        {
            // TODO populate with user's timezone
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(1);
        }
    }
}
