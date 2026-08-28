#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class GetClientsDTO : RequestStatus
    {
        public List<Client> List { get; set; }

        public decimal Total { get; set; }

        public GetClientsDTO()
        {
            List = new List<Client>();
        }
    }

    public class GetClientDTO : RequestStatus
    {
        public Client Client { get; set; }
    }

    public class SaveClientDTO : RequestStatus { }

    public class UpdateClientDTO : RequestStatus { }

    public class DeleteClientDTO : RequestStatus { }

    public class IsGetClientAllowedDTO : RequestStatus { }

    public class AllowSaveClientDTO : RequestStatus { }

}
