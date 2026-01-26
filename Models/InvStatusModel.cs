using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    public class InvStatusModel
    {
        [Display(Name = "Status ID")]
        [Key]
        public int ItemStatusID { get; set; }

        [Display(Name = "Status Name")]
        public string? ItemStatus { get; set; }

        [Display(Name = "Status Description")]
        public string? ItemStatusDescription { get; set; }

    }
}
