using System.ComponentModel.DataAnnotations;

namespace InEightDMS.Web.Models
{
    public class ProjectCreateViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Project Name")]
        public string Name { get; set; }

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Project Manager")]
        public int ManagerId { get; set; }
    }

    public class AddUserToProjectViewModel
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [Display(Name = "User")]
        public int UserId { get; set; }
    }
}
