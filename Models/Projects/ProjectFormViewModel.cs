#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Entities;
namespace VontaGenta.Models
{
    public class ProjectFormViewModel : IHasClientsSelectLists
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        [Display(Name = "Client")]
        public int ClientID { get; set; }
        public SelectList Clients { get; set; }
        public List<Client> ClientList { get; set; }

        public ProjectFormViewModel()
        {
            // TODO populate with user's timezone
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(1);
        }
    }
}
