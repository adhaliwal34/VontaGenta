using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Entities;

namespace VontaGenta.Models
{
    public interface IHasClientsSelectLists
    {
        public List<Client> ClientList { get; set; }
        SelectList Clients { get; set; }
    }
}