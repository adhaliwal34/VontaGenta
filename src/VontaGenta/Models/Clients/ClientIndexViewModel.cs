#nullable disable

namespace VontaGenta.Models
{
    public class ClientIndexViewModel
    {
        // For the field labels
        public ClientDetailsViewModel Client { get; set; }
        public List<ClientDetailsViewModel> List { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEditOrDelete { get; set; }

        public ClientIndexViewModel()
        {
            List = new List<ClientDetailsViewModel>();
        }
    }
}
