using Microsoft.AspNetCore.Mvc.Rendering;

using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    // Dedicated view model for editing — only contains editable/needed properties
    public class InvShortTermEditModel
    {
        // This section for fields in the Master Table.

        [Required]
        public int InventoryRecid { get; set; }

        // Select List __________________________________________
        // Classification value (keep the bound value if you plan to use it later).
        [Display(Name = "Classification / Usage")]
        public int ClassificationID { get; set; }
        
        [Display(Name = "Classification Name")]
        public string? Classification { get; set; }

        // SelectList for Classifications - not stored in DB, just for the view
        public SelectList? ClassificationOptions { get; set; }
        // End Select List ______________________________________


        // Master table Editable Fields for read and write.
        [Display(Name = "Storage Location")]
        public string? StorageLocation { get; set; }  // read only

        [Display(Name = "long Term Reason")]
        public string? LongTermReason { get; set; }   // read only     

        [Display(Name = "Target Duration (days)")]
        public int? TargetDuration { get; set; }

        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }

        [Display(Name = "Part Description")]
        public string? PartDescription { get; set; }

        [Display(Name = "Model / Variant")]
        public string? Model_Variant { get; set; }

        [Display(Name = "Revision Level")]
        public string? RevLevel { get; set; }

        [Display(Name = "Program Name")]
        [StringLength(200)]
        public string? ProgramName { get; set; }

        [Display(Name = "Unit Measure")]
        public string? UM { get; set; }

        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [Display(Name = "UUT Number")]
        public string? UUTNumber { get; set; }

        // Select List __________________________________________
        // Customer value (keep the bound value if you plan to use it later).
        [Display(Name = "CustomerID")]
        public int? CustomerRecID { get; set; }

        [Display(Name = "Customer")]
        public string? CustomerName { get; set; }
        
        // SelectList for Customers - not stored in DB, just for the view
        public SelectList? CustomerOptions { get; set; }
        //end Select List ______________________________________

        // Select List __________________________________________
        // ProgramPhase value (keep the bound value if you plan to use it later).
        [Display(Name = "PhaseID")]
        public int? ProgramPhaseID { get; set; }

        [Display(Name = "Phase Name")]
        public string? PhaseName { get; set; }
        
        // SelectList for ProgramPhases - not stored in DB, just for the view
        public SelectList? PhaseOptions { get; set; }
        //end Select List ______________________________________


        [Display(Name = "First Date into Storage")]
        public DateTime? FirstDateIn { get; set; }

        [Display(Name = "General Comments")]
        public string? GeneralComment { get; set; }

        // This section for fields in the CheckOut Table.
        // Also include CheckoutRecid so we can update the correct InventoryCheckOut row
        public int? CheckoutRecid { get; set; }
        public int? ShelfRecid { get; set; }

        [Display(Name = "Shelf Name")]
        public string? ShelfName { get; set; }

        [Display(Name = "Bin Number")]
        public int? BinNum { get; set; }

        [Display(Name = "LongTerm Storage Locator Number")]
        public string? LTStorageNum { get; set; }

        // Select List __________________________________________
        // Owner value (keep the bound value if you plan to use it later).       
        [Display(Name = "Owner ID")]
        public int? OwnerIDNum { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        // SelectList for ProgramPhases - not stored in DB, just for the view
        public SelectList? OwnerOptions { get; set; }

        // computed Owner property, trims blanks and avoids double spaces
        [Display(Name = "Owner")]
        public string Owner =>
            string.Join(" ", new[] { FirstName?.Trim(), LastName?.Trim() }.Where(s => !string.IsNullOrEmpty(s)));
        //end Select List ______________________________________


        // Read-only helper fields shown in UI but not editable
        [Display(Name = "Current Quantity")]
        public int? FinalQty { get; set; }

        public int LogbookCount { get; set; }

        [Display(Name = "Date In")]
        public DateTime? DateIn { get; set; }

        [Display(Name = "Time in Storage")]
        public int? CurrDuration { get; set; }
    }
}