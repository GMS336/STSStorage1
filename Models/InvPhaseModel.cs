using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    public class InvPhaseModel
    {
        [Display(Name = "Phase ID")]
        [Key]
        public int ProgramPhaseID { get; set; }

        [Display(Name = "Phase Name")]
        public string? PhaseName { get; set; }

        [Display(Name = "Phase Description")]
        public string? PhaseDescription { get; set; }

    }
}
