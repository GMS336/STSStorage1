using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    public class InvClassificationModel
    {
        [Display(Name = "Classification ID")]
        [Key]
        public int ClassificationID { get; set; }

        [Display(Name = "Classification Name")]
        public string? Classification { get; set; }

        [Display(Name = "Classification Description")]
        public string? ClassificationDescription { get; set; }

    }
}
