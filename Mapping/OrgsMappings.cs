using VontaGenta.Entities;
using VontaGenta.Models;
using VontaGenta.Services;

namespace VontaGenta.Mapping
{
    public static class OrgsMappings
    {
        public static OrgDetailsViewModel ToDetailsViewModel(this OrgDTO source)
        {
            var dest = new OrgDetailsViewModel();
            source.MapTo(dest);
            return dest;
        }

        public static void MapTo(this Org source, OrgDetailsViewModel dest)
        {
            dest.ID = source.ID;
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.UserName = source.User?.Name;
        }

        public static void MapTo(this OrgDTO source, OrgDetailsViewModel dest)
        {
            ((Org)source).MapTo(dest);
            dest.IsActive = source.IsActive;
        }

        public static void MapTo(this Org source, OrgEditViewModel dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.UserName = source.User?.Name;
        }

        public static void MapTo(this OrgFormViewModel source, Org dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
        }

        public static OrgDTO ToDTO(this Org source)
        {
            return new OrgDTO
            {
                ID = source.ID,
                Name = source.Name,
                Description = source.Description,
                AppUserID = source.AppUserID,
                User = source.User
            };
        }

        public static void MapTo(this Org source, OrgAddInviteViewModel dest)
        {
            dest.ID = source.ID;
            dest.Name = source.Name;
            dest.Description = source.Description;
        }

        public static void MapTo(this OrgInvite source, OrgInviteDeleteConfirmationViewModel dest)
        {
            dest.ID = source.ID;
            dest.FromUserName = source.Org?.User?.Name;
            dest.OrgName = source.Org?.Name;
            dest.OrgDescription = source.Org?.Description;
        }

        // When editing an org keep the existing user. It should not be overwritten.
        public static void MapTo(this Org source, Org dest)
        {
            dest.ID = source.ID;
            dest.Name = source.Name;
            dest.Description = source.Description;
        }
    }
}
