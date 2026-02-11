using Microsoft.EntityFrameworkCore;

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace STSStorage1.Models
{
    [Keyless]
    public class InvMasterModel
    {
        [Display(Name = "Record ID")]
        public int InventoryRecid { get; set; }

        [Display(Name = "Program Name")]
        public string? ProgramName { get; set; }

        public int? ShelfRecid { get; set; }

        [Display(Name = "Shelf Name")]
        public string? ShelfName { get; set; }

        [Display(Name = "Bin Number")]
        public int? BinNum { get; set; }

        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }

        [Display(Name = "Part Description")]
        public string? PartDescription { get; set; }
        
        [Display(Name = "Model / Variant")]
        public string? Model_Variant { get; set; }

        [Display(Name = "Revision Level")]
        public string? RevLevel { get; set; }

        [Display(Name = "Unit Measure")]
        public string? UM { get; set; }

        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }

        [Display(Name = "UUT Number")]
        public string? UUTNumber { get; set; }

        public int? OwnerIDNum { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

// computed Owner property, trims blanks and avoids double spaces
        [Display(Name = "Owner")]
        public string Owner =>
            string.Join(" ", new[] { FirstName?.Trim(), LastName?.Trim() }.Where(s => !string.IsNullOrEmpty(s)));

        [Display(Name = "Current Quantity")]
        public int? FinalQty { get; set; }

        [Display(Name = "Date In")]
        public DateTime? DateIn { get; set; }

        [Display(Name = "Time in Storage")]
        public int? CurrDuration { get; set; }

        [Display(Name = "First Date into Storage")]
        public DateTime? FirstDateIn { get; set; }

        [Display(Name = "General Comments")]
        public string? GeneralComment { get; set; }

        public int? cntAll { get; set; }

        // SP also returned CheckoutRecid; include it to support editing the correct checkout row
        public int? CheckoutRecid { get; set; }

        // TargetDuration included in SP
        public int? TargetDuration { get; set; }
    }
}

