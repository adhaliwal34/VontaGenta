using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public static class ClientsMappings
    {
        public static ClientDetailsViewModel ToDetailsViewModel(this Client source)
        {
            var dest = new ClientDetailsViewModel();
            source.MapTo(dest);
            return dest;
        }

        public static void MapTo(this Client source, ClientDetailsViewModel dest)
        {
            dest.ID = source.ID;
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.UserName = source.User?.Name;
        }

        public static void MapTo(this Client source, ClientFormViewModel dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
        }

        public static void MapTo(this ClientFormViewModel source, Client dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
        }

        // When editing a client keep the existing user and org. These should not be overwritten.
        public static void MapTo(this Client source, Client dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
        }
    }
}
