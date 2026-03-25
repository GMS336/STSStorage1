using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    [Keyless]
    public class InvSearchResultModel
    {
        public string? StorageLocation { get; set; }

        [Display(Name = "Record ID")]
        public int InventoryRecid { get; set; }

        [Display(Name = "Part Number")]
        public string? PartNumber { get; set; }

        [Display(Name = "Part Description")]
        public string? PartDescription { get; set; }

        [Display(Name = "Model / Variant")]
        public string? Model_Variant { get; set; }

        [Display(Name = "Revision Level")]
        public string? RevLevel { get; set; }

        public int? OwnerIDNum { get; set; }

        [Display(Name = "Program Name")]
        public string? ProgramName { get; set; }

        public int? ProgramPhaseID { get; set; }
        public int? CustomerRecID { get; set; }

        [Display(Name = "Customer")]
        public string? CustomerName { get; set; }

        [Display(Name = "Balance")]
        public int? Balance { get; set; }

        public string? UM { get; set; }
        public int? ClassificationID { get; set; }

        public string? SerialNumber { get; set; }

        [Display(Name = "UUT Number")]
        public string? UUTNumber { get; set; }

        [Display(Name = "General Comments")]
        public string? GeneralComment { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Display(Name = "Owner")]
        public string Owner =>
            string.Join(" ", new[] { FirstName?.Trim(), LastName?.Trim() }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        public int? cntAll { get; set; }

        [Display(Name = "Logbook")]
        public int? LogbookCount { get; set; }

        [Display(Name = "Date In")]
        public DateTime? DateIn { get; set; }

        [Display(Name = "Current Duration")]
        public int? CurrDuration { get; set; }

        public int? RequestorIDNum { get; set; }

        [Display(Name = "Long Term Ref Number")]
        public string? LTStorageNum { get; set; }

        [Display(Name = "Storage Location")]
        public string? LocationHistory { get; set; }

        public int? ShelfRecID { get; set; }

        [Display(Name = "Shelf Name")]
        public string? ShelfName { get; set; }

        [Display(Name = "Bin Number")]
        public int? BinNum { get; set; }

        public string? WONum { get; set; }

        public int? ItemStatusID { get; set; }

        [Display(Name = "Status")]
        public string? ItemStatus { get; set; }
    }
}
