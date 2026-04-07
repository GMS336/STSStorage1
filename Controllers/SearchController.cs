using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Helpers;
using STSStorage1.Models;

namespace STSStorage1.Controllers
{
    // SearchController:
    // Hosts the search UI + search execution endpoints for inventory/storage records.
    // Implements the typical flow:
    //   1) GET search form
    //   2) POST search criteria (validate + normalize)
    //   3) Redirect to GET results page (supports paging/sorting via query string)
    public class SearchController : BaseController
    {
        // EF Core DbContext used to read dropdown data and execute the stored procedure search.
        private readonly STSStorage1Context _context;

        // Constructor injection: provides the controller with the application's DbContext.
        public SearchController(STSStorage1Context context)
        {
            _context = context;
        }

        // ----------------------------
        // GET /Search (default route)
        // ----------------------------
        // Entry point for the Search feature.
        // Delegates to SearchStorage() so Index is just a friendly default URL.
        [HttpGet]
        public async Task<IActionResult> Index(string? noItemFound = null)
        {
            return await SearchStorage(noItemFound);
        }

        // ----------------------------
        // GET /Search/SearchStorage
        // ----------------------------
        // Renders the search form page.
        // - Builds an empty SearchCriteriaModel
        // - Loads dropdown lists (customers, owners, phases, statuses, shelves)
        // - Optionally sets a flag to show "no items found" message when redirected back.
        [HttpGet]
        public async Task<IActionResult> SearchStorage(string? noItemFound = null)
        {
            var model = new SearchCriteriaModel();
            await LoadSearchDropdownOptions(model);

            ViewBag.NoItemFound = string.Equals(noItemFound, "None", StringComparison.OrdinalIgnoreCase);
            return View("~/Views/Search/SearchStorage.cshtml", model);
        }

        // ----------------------------
        // POST /Search/SearchStorage
        // ----------------------------
        // Accepts submitted search criteria from the search form.
        // - Validates that at least one search field was provided
        // - Normalizes date range (if BeginDate provided but EndDate missing, set EndDate = BeginDate)
        // - Redirects to SearchIndex (GET) with the criteria in the query string so results are bookmarkable,
        //   and paging/sorting works via links.
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

            // If only one date was provided, treat it as a single-day range.
            if (model.BeginDate.HasValue && !model.EndDate.HasValue)
            {
                model.EndDate = model.BeginDate;
            }

            // Redirect to results endpoint with default sort + paging parameters.
            // IMPORTANT: use sc_* parameter names to avoid collisions with edit models in other pages.
            return RedirectToAction(nameof(SearchIndex), new
            {
                // criteria (sc_*)
                sc_inventoryRecid = model.InventoryRecid,
                sc_storageLocation = model.StorageLocation,
                sc_ownerIDNum = model.OwnerIDNum,
                sc_partNumber = model.PartNumber,
                sc_partDescription = model.PartDescription,
                sc_customerRecID = model.CustomerRecID,
                sc_programName = model.ProgramName,
                sc_model_Variant = model.Model_Variant,
                sc_programPhaseID = model.ProgramPhaseID,
                sc_itemStatusID = model.ItemStatusID,
                sc_ltStorageNum = model.LTStorageNum,
                sc_binNum = model.BinNum,
                sc_shelfRecid = model.ShelfRecid,
                sc_serialNumber = model.SerialNumber,
                sc_uutNumber = model.UUTNumber,
                sc_beginDate = model.BeginDate,
                sc_endDate = model.EndDate,
                sc_generalComment = model.GeneralComment,

                // state
                sortOrder = "InventoryRecid",
                sortDir = "desc",
                page = 1,
                pageSize = 10
            });
        }

        // ----------------------------
        // GET /Search/SearchIndex
        // ----------------------------
        // Executes the search and renders the results page.
        // - Accepts criteria + paging/sorting parameters via query string
        // - Normalizes sort direction + date range
        // - Sets a larger DB command timeout (temporary mitigation for long-running queries)
        // - Preloads lookup dictionaries (ID -> Name) used only to display the chosen criteria nicely
        // - Calls dbo.spFILTERSearch stored procedure through EF Core FromSqlRaw
        // - Wraps results in PaginatedList and returns SearchIndex view
        // ----------------------------
        // GET /Search/SearchIndex
        // ----------------------------
        [HttpGet]
        public async Task<IActionResult> SearchIndex(
            // Preferred inputs (sc_*)
            int? sc_inventoryRecid = null,
            string? sc_storageLocation = null,
            int? sc_ownerIDNum = null,
            string? sc_partNumber = null,
            string? sc_partDescription = null,
            int? sc_customerRecID = null,
            string? sc_programName = null,
            string? sc_model_Variant = null,
            int? sc_programPhaseID = null,
            int? sc_itemStatusID = null,
            string? sc_ltStorageNum = null,
            int? sc_binNum = null,
            int? sc_shelfRecid = null,
            string? sc_serialNumber = null,
            string? sc_uutNumber = null,
            DateTime? sc_beginDate = null,
            DateTime? sc_endDate = null,
            string? sc_generalComment = null,

            // Legacy inputs (optional; keeps old links working)
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

            // state
            string sortOrder = "InventoryRecid",
            string sortDir = "desc",
            int page = 1,
            int pageSize = 10
        )
        {
            // Ensure sort values are always valid and consistent.
            sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "InventoryRecid" : sortOrder;
            sortDir = (sortDir ?? "desc").ToLower() == "desc" ? "desc" : "asc";

            // Canonicalize: prefer sc_*; fallback to legacy
            int invRecid = sc_inventoryRecid ?? inventoryRecid ?? 0;
            string storLoc = sc_storageLocation ?? storageLocation ?? "";
            int ownId = sc_ownerIDNum ?? ownerIDNum ?? 0;
            string pn = sc_partNumber ?? partNumber ?? "";
            string pd = sc_partDescription ?? partDescription ?? "";
            int custId = sc_customerRecID ?? customerRecID ?? 0;
            string prog = sc_programName ?? programName ?? "";
            string mv = sc_model_Variant ?? model_Variant ?? "";
            int phaseId = sc_programPhaseID ?? programPhaseID ?? 0;
            int statusId = sc_itemStatusID ?? itemStatusID ?? 0;
            string ltNum = sc_ltStorageNum ?? ltStorageNum ?? "";
            int bNum = sc_binNum ?? binNum ?? 0;
            int shRecid = sc_shelfRecid ?? shelfRecid ?? 0;
            string sn = sc_serialNumber ?? serialNumber ?? "";
            string uut = sc_uutNumber ?? uutNumber ?? "";
            DateTime? bDate = sc_beginDate ?? beginDate;
            DateTime? eDate = sc_endDate ?? endDate;
            string gc = sc_generalComment ?? generalComment ?? "";

            // If only one date was provided, treat it as a single-day range.
            if (bDate.HasValue && !eDate.HasValue)
            {
                eDate = bDate;
            }

            _context.Database.SetCommandTimeout(130);

            // ========= Lookups for display =========
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

            // ========= Search execution =========
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
                    storLoc,
                    invRecid,
                    ownId,
                    pn,
                    pd,
                    custId,
                    prog,
                    mv,
                    phaseId,
                    statusId,
                    ltNum,
                    bNum,
                    shRecid,
                    sn,
                    uut,
                    gc,
                    bDate ?? (object)DBNull.Value,
                    eDate ?? (object)DBNull.Value,
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

            int totalCount = rows.FirstOrDefault()?.cntAll ?? rows.Count;
            var paged = new PaginatedList<InvSearchResultModel>(rows, totalCount, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortDir = sortDir;

            // Reconstruct criteria for the view (canonical values)
            ViewBag.Criteria = new SearchCriteriaModel
            {
                InventoryRecid = invRecid == 0 ? (int?)null : invRecid,
                StorageLocation = string.IsNullOrWhiteSpace(storLoc) ? null : storLoc,
                OwnerIDNum = ownId == 0 ? (int?)null : ownId,
                PartNumber = string.IsNullOrWhiteSpace(pn) ? null : pn,
                PartDescription = string.IsNullOrWhiteSpace(pd) ? null : pd,
                CustomerRecID = custId == 0 ? (int?)null : custId,
                ProgramName = string.IsNullOrWhiteSpace(prog) ? null : prog,
                Model_Variant = string.IsNullOrWhiteSpace(mv) ? null : mv,
                ProgramPhaseID = phaseId == 0 ? (int?)null : phaseId,
                ItemStatusID = statusId == 0 ? (int?)null : statusId,
                LTStorageNum = string.IsNullOrWhiteSpace(ltNum) ? null : ltNum,
                BinNum = bNum == 0 ? (int?)null : bNum,
                ShelfRecid = shRecid == 0 ? (int?)null : shRecid,
                SerialNumber = string.IsNullOrWhiteSpace(sn) ? null : sn,
                UUTNumber = string.IsNullOrWhiteSpace(uut) ? null : uut,
                BeginDate = bDate,
                EndDate = eDate,
                GeneralComment = string.IsNullOrWhiteSpace(gc) ? null : gc
            };

            return View("~/Views/Search/SearchIndex.cshtml", paged);
        }

        // ----------------------------
        // Helper: LoadSearchDropdownOptions
        // ----------------------------
        // Populates SelectList dropdown options on the search form model:
        // - Customers, Phases, Owners, Statuses, Shelves
        // The lists are ordered for user-friendly display and set the selected value based on the model.
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