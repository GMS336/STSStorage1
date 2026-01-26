using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    public class InvCustomerModel
    {
        [Display(Name = "Customer Record ID")]
        [Key]
        public int CustomerRecID { get; set; }

        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        [Display(Name = "Customer Code")]
        public string? CustomerCode { get; set; }

        [Display(Name = "Customer Location")]
        public string? CustomerLocation { get; set; }

    }
}