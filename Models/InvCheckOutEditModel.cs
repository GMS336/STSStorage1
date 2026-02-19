using Microsoft.AspNetCore.Mvc.Rendering;

using System.ComponentModel.DataAnnotations;

namespace STSStorage1.Models
{
    public class InvCheckOutEditModel
    {
        // Primary Keys
        public int CheckOutRecid { get; set; }
        public int InventoryRecid { get; set; }

        // Requestor Information
        [Display(Name = "Requestor ID")]
        public int? RequestorIDNum { get; set; }

        [Display(Name = "Requestor Name")]
        public string? RequestorName { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Request Information
        [Display(Name = "Request Date")]
        public DateTime? RequestDate { get; set; }

        [Display(Name = "Request Form Type")]
        public string? RequestFormType { get; set; }

        [Display(Name = "Date Needed")]
        public DateTime? NeedDate { get; set; }

        // Check In Information
        [Display(Name = "Date In")]
        public DateTime? DateIn { get; set; }

        [Display(Name = "Quantity In")]
        public int? QtyIn { get; set; }

        [Display(Name = "Comments - Stored")]
        public string? CommentsStored { get; set; }

        // Check Out Information
        [Display(Name = "Date Out")]
        public DateTime? DateOut { get; set; }

        [Display(Name = "Quantity Out")]
        public int? QtyOut { get; set; }

        [Display(Name = "Comments - Retrieval")]
        public string? CommentRetrieval { get; set; }

        // Location Information
        [Display(Name = "Location History")]
        public string? LocationHistory { get; set; }

        [Display(Name = "Shelf")]
        public int? ShelfRecid { get; set; }

        [Display(Name = "Shelf Name")]
        public string? ShelfName { get; set; }

        [Display(Name = "Bin Number")]
        public int? BinNum { get; set; }

        [Display(Name = "LT Storage Number")]
        public string? LTStorageNum { get; set; }

        // Item Status
        [Display(Name = "Item Status ID")]
        public int? ItemStatusID { get; set; }

        [Display(Name = "Item Status")]
        public string? ItemStatus { get; set; }

        // Other Information
        [Display(Name = "Work Order Number")]
        public string? WONum { get; set; }

        [Display(Name = "Oil Drained?")]
        public string? OilCheck { get; set; }

        // Balance Properties - ADDED THIS
        [Display(Name = "Running Balance")]
        public int RunningBalance { get; set; }

        [Display(Name = "Balance")]
        public int Balance { get; set; }

        // SelectList properties for dropdowns
        public SelectList? ShelfOptions { get; set; }
        public SelectList? RequestorOptions { get; set; }
        public SelectList? ItemStatusOptions { get; set; }
        public SelectList? BinOptions { get; set; }
    }
}