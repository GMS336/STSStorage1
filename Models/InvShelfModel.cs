using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    public class InvShelfModel
    {
        [Display(Name = "Shelf ID")]
        [Key]
        public int ShelfRecid { get; set; }

        [Display(Name = "Shelf Name")]
        public string? ShelfName { get; set; }

        [Display(Name = "Shelf Description")]
        public string? ShelfDescription { get; set; }
        
        [Display(Name = "Storage Site")]
        public string? StorageLocationSite { get; set; }

    }
}
