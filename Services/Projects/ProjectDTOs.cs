#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class GetProjectsDTO : RequestStatus
    {
        public List<Project> List { get; set; }

        public decimal Total { get; set; }

        public GetProjectsDTO()
        {
            List = new List<Project>();
        }
    }

    public class GetProjectDTO : RequestStatus
    {
        public Project Project { get; set; }
    }

    public class SaveProjectDTO : RequestStatus { }

    public class UpdateProjectDTO : RequestStatus { }

    public class DeleteProjectDTO : RequestStatus { }

    public class IsGetProjectAllowedDTO : RequestStatus { }

    public class AllowSaveProjectDTO : RequestStatus { }

}
