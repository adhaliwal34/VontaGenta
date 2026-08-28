#nullable disable

namespace VontaGenta.Models
{
    public class ProjectIndexViewModel
    {
        // For the field labels
        public ProjectDetailsViewModel Project { get; set; }
        public List<ProjectDetailsViewModel> List { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEditOrDelete { get; set; }

        public ProjectIndexViewModel()
        {
            List = new List<ProjectDetailsViewModel>();
        }
    }
}
