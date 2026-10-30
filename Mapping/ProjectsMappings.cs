using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public static class ProjectsMappings
    {
        public static ProjectDetailsViewModel ToDetailsViewModel(this Project source)
        {
            var dest = new ProjectDetailsViewModel();
            source.MapTo(dest);
            return dest;
        }

        public static void MapTo(this Project source, ProjectDetailsViewModel dest)
        {
            dest.ID = source.ID;
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.StartDate = source.StartDate;
            dest.EndDate = source.EndDate;
            dest.UserName = source.User?.Name;
            dest.ClientName = source.Client?.Name;
        }

        public static void MapTo(this Project source, ProjectFormViewModel dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.StartDate = source.StartDate;
            dest.EndDate = source.EndDate;
            dest.ClientID = source.ClientID;
        }

        public static void MapTo(this ProjectFormViewModel source, Project dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.StartDate = source.StartDate;
            dest.EndDate = source.EndDate;
            dest.ClientID = source.ClientID;
        }

        // When editing a project keep the existing user. This should not be overwritten.
        public static void MapTo(this Project source, Project dest)
        {
            dest.ID = source.ID;
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.StartDate = source.StartDate;
            dest.EndDate = source.EndDate;
            dest.ClientID = source.ClientID;
        }
    }
}
