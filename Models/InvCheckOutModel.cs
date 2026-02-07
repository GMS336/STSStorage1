using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STSStorage1.Models
{
    [Keyless]
    public class InvCheckOutModel
    {
        [Display(Name = "CheckOut Record ID")]
        public int CheckOutRecid { get; set; }

        [Display(Name = "Inventory Record ID")]
        public int InventoryRecid { get; set; }

        [Display(Name = "Request Date")]
        public DateTime? RequestDate { get; set; }

        [Display(Name = "Requestor ID")]
        public int? RequestorIDNum { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Computed Requestor Name property
        [Display(Name = "Requestor Name")]
        public string RequestorName =>
            string.Join(" ", new[] { FirstName?.Trim(), LastName?.Trim() }.Where(s => !string.IsNullOrEmpty(s)));

        [Display(Name = "Request Form Type")]
        public string? RequestFormType { get; set; }

        [Display(Name = "Date In")]
        public DateTime? DateIn { get; set; }

        [Display(Name = "Quantity In")]
        public int? QtyIn { get; set; }

        [Display(Name = "Date Out")]
        public DateTime? DateOut { get; set; }

        [Display(Name = "Quantity Out")]
        public int? QtyOut { get; set; }

        [Display(Name = "Location History")]
        public string? LocationHistory { get; set; }

        [Display(Name = "LT Storage Number")]
        public string? LTStorageNum { get; set; }

        public int? ShelfRecid { get; set; }

        [Display(Name = "Shelf Name")]
        public string? ShelfName { get; set; }

        [Display(Name = "Bin Number")]
        public int? BinNum { get; set; }

        [Display(Name = "Work Order Number")]
        public string? WONum { get; set; }

        [Display(Name = "Item Status")]
        public string? ItemStatus { get; set; }

        [Display(Name = "Oil Check")]
        public string? OilCheck { get; set; }

        [Display(Name = "Comments Stored")]
        public string? CommentsStored { get; set; }

        [Display(Name = "Comment Retrieval")]
        public string? CommentRetrieval { get; set; }

        [Display(Name = "Target Duration")]
        public int? TargetDuration { get; set; }

        // ===== Computed Properties for Display =====
        // These are calculated in the controller and not mapped from database
        // [NotMapped] tells EF Core to ignore these during database operations

        [NotMapped]
        [Display(Name = "Date Moved")]
        public DateTime? DateMoved { get; set; }

        [NotMapped]
        [Display(Name = "Quantity Moved")]
        public int QtyMoved { get; set; }

        [NotMapped]
        [Display(Name = "Running Balance")]
        public int RunningBalance { get; set; }

        [NotMapped]
        [Display(Name = "Oil Check Display")]
        public string OilCheckDisplay { get; set; } = "No";

        [NotMapped]
        [Display(Name = "Formatted Comments")]
        public string? FormattedComments { get; set; }
    }
}