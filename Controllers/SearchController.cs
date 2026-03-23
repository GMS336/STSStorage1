using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Helpers;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    public class SearchController : BaseController
    {
        private readonly STSStorage1Context _context;

        public SearchController(STSStorage1Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? noItemFound = null)
        {
            return await SearchStorage(noItemFound);
        }

        [HttpGet]
        public async Task<IActionResult> SearchStorage(string? noItemFound = null)
        {
            var model = new SearchCriteriaModel();
            await LoadSearchDropdownOptions(model);

            ViewBag.NoItemFound = string.Equals(noItemFound, "None", StringComparison.OrdinalIgnoreCase);
            return View("~/Views/Search/SearchStorage.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchStorage(SearchCriteriaModel model)
        {
            if (model == null || model.IsEmpty())
            {
                ModelState.AddModelError(string.Empty, "Please enter at least one search field.");
                model ??= new SearchCriteriaModel();
                await LoadSearchDropdownOptions(model);
                return View("~/Views/Search/SearchStorage.cshtml", model);
            }

            if (model.BeginDate.HasValue && !model.EndDate.HasValue)
            {
                model.EndDate = model.BeginDate;
            }

            return RedirectToAction(nameof(SearchIndex), new
            {
                // criteria
                model.InventoryRecid,
                model.StorageLocation,
                model.OwnerIDNum,
                model.PartNumber,
                model.PartDescription,
                model.CustomerRecID,
                model.ProgramName,
                model.Model_Variant,
                model.ProgramPhaseID,
                model.ItemStatusID,
                model.LTStorageNum,
                model.BinNum,
                model.ShelfRecid,
                model.SerialNumber,
                model.UUTNumber,
                model.BeginDate,
                model.EndDate,
                model.GeneralComment,

                // state
                sortOrder = "InventoryRecid",
                sortDir = "desc",
                page = 1,
                pageSize = 10
            });
        }

        [HttpGet]
        public async Task<IActionResult> SearchIndex(
            int? inventoryRecid = null,
            string? storageLocation = null,
            int? ownerIDNum = null,
            string? partNumber = null,
            string? partDescription = null,
            int? customerRecID = null,
            string? programName = null,
            string? model_Variant = null,
            int? programPhaseID = null,
            int? itemStatusID = null,
            string? ltStorageNum = null,
            int? binNum = null,
            int? shelfRecid = null,
            string? serialNumber = null,
            string? uutNumber = null,
            DateTime? beginDate = null,
            DateTime? endDate = null,
            string? generalComment = null,

            string sortOrder = "InventoryRecid",
            string sortDir = "desc",
            int page = 1,
            int pageSize = 10
        )
        {
            sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "InventoryRecid" : sortOrder;
            sortDir = (sortDir ?? "desc").ToLower() == "desc" ? "desc" : "asc";

            if (beginDate.HasValue && !endDate.HasValue)
            {
                endDate = beginDate;
            }

            _context.Database.SetCommandTimeout(30);

            // ========= NEW: Lookups for ID -> Name for the "criteria cards" =========
            // We only need these for display. We keep your existing SearchCriteriaModel unchanged.
            // Note: using Dictionary<int,string> so the view can quickly TryGetValue(...)
            ViewBag.LookupOwnerNames = await _context.InventoryUsers
                .AsNoTracking()
                .Select(u => new
                {
                    Id = u.MyID,
                    Name = ((u.LastName ?? "") + ", " + (u.FirstName ?? "")).Trim().Trim(',', ' ')
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            ViewBag.LookupCustomerNames = await _context.InventoryCustomer
                .AsNoTracking()
                .Select(c => new { Id = c.CustomerRecID, Name = c.CustomerName })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            ViewBag.LookupPhaseNames = await _context.InventoryProjectPhase
                .AsNoTracking()
                .Select(p => new { Id = p.ProgramPhaseID, Name = p.PhaseName })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            ViewBag.LookupStatusNames = await _context.InventoryItemStatus
                .AsNoTracking()
                .Select(s => new { Id = s.ItemStatusID, Name = s.ItemStatus })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            ViewBag.LookupShelfNames = await _context.InventoryShelf
                .AsNoTracking()
                .Select(s => new { Id = s.ShelfRecid, Name = s.ShelfName })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            // ========= Your existing search execution =========
            var rows = await _context.InvSearchResults
                .FromSqlRaw(
                    @"EXEC dbo.spFILTERSearch
                        @StorageLocation = {0},
                        @InventoryRecid = {1},
                        @OwnerIDNum = {2},
                        @PartNumber = {3},
                        @PartDescription = {4},
                        @CustomerRecID = {5},
                        @ProgramName = {6},
                        @Model_Variant = {7},
                        @ProgramPhaseID = {8},
                        @ItemStatusID = {9},
                        @LTStorageNum = {10},
                        @BinNum = {11},
                        @ShelfRecid = {12},
                        @SerialNumber = {13},
                        @UUTNumber = {14},
                        @GeneralComment = {15},
                        @BeginDate = {16},
                        @EndDate = {17},
                        @SortValue = {18},
                        @SortDir = {19},
                        @Page = {20},
                        @PageSize = {21}",
                    storageLocation ?? "",
                    inventoryRecid ?? 0,
                    ownerIDNum ?? 0,
                    partNumber ?? "",
                    partDescription ?? "",
                    customerRecID ?? 0,
                    programName ?? "",
                    model_Variant ?? "",
                    programPhaseID ?? 0,
                    itemStatusID ?? 0,
                    ltStorageNum ?? "",
                    binNum ?? 0,
                    shelfRecid ?? 0,
                    serialNumber ?? "",
                    uutNumber ?? "",
                    generalComment ?? "",
                    beginDate ?? (object)DBNull.Value,
                    endDate ?? (object)DBNull.Value,
                    sortOrder,
                    sortDir,
                    page,
                    pageSize
                )
                .AsNoTracking()
                .ToListAsync();

            if (rows.Count == 0)
            {
                return RedirectToAction(nameof(SearchStorage), new { noItemFound = "None" });
            }

            int totalCount = rows.First().cntAll ?? rows.Count;

            var paged = new PaginatedList<InvSearchResultModel>(rows, totalCount, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortDir = sortDir;

            ViewBag.Criteria = new SearchCriteriaModel
            {
                InventoryRecid = inventoryRecid,
                StorageLocation = storageLocation,
                OwnerIDNum = ownerIDNum,
                PartNumber = partNumber,
                PartDescription = partDescription,
                CustomerRecID = customerRecID,
                ProgramName = programName,
                Model_Variant = model_Variant,
                ProgramPhaseID = programPhaseID,
                ItemStatusID = itemStatusID,
                LTStorageNum = ltStorageNum,
                BinNum = binNum,
                ShelfRecid = shelfRecid,
                SerialNumber = serialNumber,
                UUTNumber = uutNumber,
                BeginDate = beginDate,
                EndDate = endDate,
                GeneralComment = generalComment
            };

            return View("~/Views/Search/SearchIndex.cshtml", paged);
        }

        private async Task LoadSearchDropdownOptions(SearchCriteriaModel model)
        {
            var customers = await _context.InventoryCustomer
                .OrderBy(c => c.CustomerName)
                .ToListAsync();
            model.CustomerOptions = new SelectList(customers, "CustomerRecID", "CustomerName", model.CustomerRecID);

            var phases = await _context.InventoryProjectPhase
                .OrderBy(p => p.PhaseName)
                .ToListAsync();
            model.PhaseOptions = new SelectList(phases, "ProgramPhaseID", "PhaseName", model.ProgramPhaseID);

            var owners = await _context.InventoryUsers
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
            var ownerList = owners.Select(o => new
            {
                MyID = o.MyID,
                FullName = $"{o.LastName}, {o.FirstName}".Trim()
            }).ToList();
            model.OwnerOptions = new SelectList(ownerList, "MyID", "FullName", model.OwnerIDNum);

            var statuses = await _context.InventoryItemStatus
                .OrderBy(s => s.ItemStatus)
                .ToListAsync();
            model.ItemStatusOptions = new SelectList(statuses, "ItemStatusID", "ItemStatus", model.ItemStatusID);

            var shelves = await _context.InventoryShelf
                .OrderBy(s => s.ShelfName)
                .ToListAsync();
            model.ShelfOptions = new SelectList(shelves, "ShelfRecid", "ShelfName", model.ShelfRecid);
        }
    }
}