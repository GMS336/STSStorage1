using Microsoft.AspNetCore.Mvc.Rendering;

namespace STSStorage1.Models
{
    public class SearchCriteriaModel
    {
        public int? InventoryRecid { get; set; }
        public string? StorageLocation { get; set; } // ShortTerm / LongTerm (optional)

        public int? OwnerIDNum { get; set; }

        public string? PartNumber { get; set; }
        public string? PartDescription { get; set; }

        public int? CustomerRecID { get; set; }

        public string? ProgramName { get; set; }
        public string? Model_Variant { get; set; }

        public int? ProgramPhaseID { get; set; }
        public int? ItemStatusID { get; set; }

        public string? LTStorageNum { get; set; }
        public int? BinNum { get; set; }
        public int? ShelfRecid { get; set; }

        public string? SerialNumber { get; set; }
        public string? UUTNumber { get; set; }

        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? GeneralComment { get; set; }

        // Dropdowns
        public SelectList? OwnerOptions { get; set; }
        public SelectList? CustomerOptions { get; set; }
        public SelectList? PhaseOptions { get; set; }
        public SelectList? ItemStatusOptions { get; set; }
        public SelectList? ShelfOptions { get; set; }

        public bool IsEmpty()
        {
            return
                (InventoryRecid == null || InventoryRecid == 0) &&
                string.IsNullOrWhiteSpace(StorageLocation) &&
                (OwnerIDNum == null || OwnerIDNum == 0) &&
                string.IsNullOrWhiteSpace(PartNumber) &&
                string.IsNullOrWhiteSpace(PartDescription) &&
                (CustomerRecID == null || CustomerRecID == 0) &&
                string.IsNullOrWhiteSpace(ProgramName) &&
                string.IsNullOrWhiteSpace(Model_Variant) &&
                (ProgramPhaseID == null || ProgramPhaseID == 0) &&
                (ItemStatusID == null || ItemStatusID == 0) &&
                string.IsNullOrWhiteSpace(LTStorageNum) &&
                (BinNum == null || BinNum == 0) &&
                (ShelfRecid == null || ShelfRecid == 0) &&
                string.IsNullOrWhiteSpace(SerialNumber) &&
                string.IsNullOrWhiteSpace(UUTNumber) &&
                (BeginDate == null) &&
                (EndDate == null) &&
                string.IsNullOrWhiteSpace(GeneralComment);
        }
    }
}