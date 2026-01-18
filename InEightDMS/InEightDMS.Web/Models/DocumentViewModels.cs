using System.ComponentModel.DataAnnotations;
using System.Web;
using InEightDMS.Domain.Entities;

namespace InEightDMS.Web.Models
{
    public class DocumentCreateViewModel
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Document Name")]
        public string Name { get; set; }

        [StringLength(2000)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Document Type")]
        public string Type { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "Tags (comma separated)")]
        public string Tags { get; set; }

        [Display(Name = "Transmittal Number")]
        public string TransmittalNumber { get; set; }
    }

    public class LinkedItemViewModel
    {
        [Required]
        public int DocumentId { get; set; }

        [Required]
        [Display(Name = "Item Type")]
        public LinkedItemType ItemType { get; set; }

        [Required]
        [Display(Name = "Item Number")]
        public string ItemNumber { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }
    }
}
