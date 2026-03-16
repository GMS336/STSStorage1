using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace STSStorage1.Models
{
    public class InvShortTermCreateModel
    {
        // ====== InventoryMaster fields (spADDNewItem params) ======
        [Required]
        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }

        [Required]
        [Display(Name = "Part Description")]
        public string? PartDescription { get; set; }

        [Required]
        [Display(Name = "Model / Variant")]
        public string? Model_Variant { get; set; }

        [Display(Name = "Revision Level")]
        public string? RevLevel { get; set; }

        [Required]
        [Display(Name = "Owner / Responsible")]
        public int? OwnerIDNum { get; set; }

        [Required]
        [Display(Name = "Program Name")]
        public string? ProgramName { get; set; }

        [Display(Name = "Customer")]
        public int? CustomerRecID { get; set; }

        [Display(Name = "Unit of Measure")]
        public string? UM { get; set; } = "Each";

        [Display(Name = "Target Duration (Days)")]
        public int? TargetDuration { get; set; } = 180;

        [Required]
        [Display(Name = "Classification / Usage")]
        public int? ClassificationID { get; set; }

        [Required]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [Display(Name = "UUT Number")]
        public string? UUTNumber { get; set; }

        [Display(Name = "General Comments")]
        public string? GeneralComment { get; set; }

        [Display(Name = "Project Phase")]
        public int? ProgramPhaseID { get; set; }

        [Required]
        [Display(Name = "Storage Location")]
        public string? StorageLocation { get; set; } = "ShortTerm";

        [Display(Name = "Long Term Reason")]
        public string? LongTermReason { get; set; }

        [Display(Name = "Log Status")]
        public string? LogStatus { get; set; } = "New";

        // ====== InventoryCheckOut fields (spADDNewItem params) ======
        [Required]
        [Display(Name = "Request Date")]
        [DataType(DataType.Date)]
        public DateTime? RequestDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Qty In")]
        public int? QtyIn { get; set; }

        [Display(Name = "Qty Out")]
        public int? QtyOut { get; set; } = 0;

        [Display(Name = "Comment In")]
        public string? CommentsStored { get; set; }

        public int? RequestorIDNum { get; set; }

        [Display(Name = "Work Orders")]
        public string? WONum { get; set; }

        [Display(Name = "Request Form Type")]
        public string? RequestFormType { get; set; } = "Return";

        [Display(Name = "Item Status")]
        public int? ItemStatusID { get; set; }

        [Required]
        [Display(Name = "Pick Up Location")]
        public string? PickUpLocation { get; set; }

        [Display(Name = "Oil Drained?")]
        public string? OilCheck { get; set; } = "Yes";

        // ============================
        // Dropdown options (Create)
        // ============================
        public SelectList? ClassificationOptions { get; set; }
        public SelectList? CustomerOptions { get; set; }
        public SelectList? PhaseOptions { get; set; }
        public SelectList? OwnerOptions { get; set; }
        public SelectList? ItemStatusOptions { get; set; }
    }
}