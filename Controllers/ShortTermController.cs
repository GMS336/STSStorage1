using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using STSStorage1.Data;
using STSStorage1.Helpers;
using STSStorage1.Models;

using System.Data;

namespace STSStorage1.Controllers
{
    public class ShortTermController : BaseController
    {
        private readonly STSStorage1Context _context;

        public ShortTermController(STSStorage1Context context)
        {
            _context = context;
        }
        //______________________________________________________________________________________________________________   

        // GET: InvShortTerm Index
        // Accept sortOrder and sortDir (asc/desc). Pass both to the stored procedure.
        public async Task<IActionResult> ShortIndex(string sortOrder = "InventoryRecid", string sortDir = "asc", int page = 1, int pageSize = 10)
        {
            // Normalize incoming values
            sortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "InventoryRecid" : sortOrder;
            sortDir = (sortDir ?? "asc").ToLower() == "desc" ? "desc" : "asc";

            // Increase the command timeout to 10 seconds
            _context.Database.SetCommandTimeout(10);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // Execute stored procedure for paged results. SP updated to accept @SortDir.
            var items = await _context.InvShortTerm
                .FromSqlRaw(
                    "EXEC spGETAllShortTermTest @SortValue = {0}, @SortDir = {1}, @Page = {2}, @PageSize = {3}",
                    sortOrder, sortDir, page, pageSize)
                .AsNoTracking()
                .ToListAsync();
            ViewBag.ItemsCount = items.Count;

            sw.Stop();
            ViewBag.DataFetchTime = sw.ElapsedMilliseconds; // Time in milliseconds

            // Always get total count from the first row's cntAll, even if it's a dummy row
            int count = items.Count != 0 && items.First().cntAll.HasValue ? items.First().cntAll.Value : 0;

            // If there are no records but count > 0, redirect to last valid page
            if (!items.Any() && count > 0 && page > 1)
            {
                int lastPage = (int)System.Math.Ceiling(count / (double)pageSize);
                return RedirectToAction("ShortIndex", new { page = lastPage, pageSize = pageSize, sortOrder = sortOrder, sortDir = sortDir });
            }

            // Remove dummy row if present (InventoryRecid is null/0)
            var realItems = items.Where(x => x.InventoryRecid != 0).ToList();

            var pagedList = new PaginatedList<InvShortTermModel>(realItems, count, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            // expose the current sortOrder and sortDir for the view to use when rendering links/indicators
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortDir = sortDir;

            return View("~/Views/ShortTerm/ShortIndex.cshtml", pagedList);
        }
//_________________________________________________________________________________________________

        // ============================
        // GET: ShortTerm/ShortCreate
        // ============================
        public async Task<IActionResult> ShortCreate(
            int? returnPage = null,
            int? returnPageSize = null,
            string? returnSortOrder = null,
            string? returnSortDir = null)
        {
            var myId = HttpContext.Session.GetInt32("MyID");
            if (!myId.HasValue || myId.Value == 0)
            {
                return RedirectToAction("LoginDb", "Account", new { timeout = true });
            }

            // Store return parameters in ViewBag for the view (same pattern as ShortEdit)
            ViewBag.ReturnPage = returnPage ?? 1;
            ViewBag.ReturnPageSize = returnPageSize ?? 10;
            ViewBag.ReturnSortOrder = returnSortOrder ?? "InventoryRecid";
            ViewBag.ReturnSortDir = returnSortDir ?? "asc";

            // Requestor display info (read-only)
            ViewBag.CurrentUserId = myId.Value;
            ViewBag.CurrentUserFullName = HttpContext.Session.GetString("FullName") ?? "";

            await PopulateShortCreateDropdowns(selectedOwnerId: myId.Value);

            // Default model values (classic ASP defaults)
            var model = new InvShortTermCreateModel
            {
                RequestorIDNum = myId.Value,
                OwnerIDNum = myId.Value,
                RequestDate = DateTime.Today,
                StorageLocation = "ShortTerm",
                TargetDuration = 180,
                UM = "Each",
                QtyOut = 0,
                LogStatus = "New",
                RequestFormType = "Return",
                OilCheck = "Yes"
            };

            return View("~/Views/ShortTerm/ShortCreate.cshtml", model);
        }

        // ============================
        // POST: ShortTerm/ShortCreate
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShortCreate(
            InvShortTermCreateModel model,
            int? returnPage = null,
            int? returnPageSize = null,
            string? returnSortOrder = null,
            string? returnSortDir = null)
        {
            var myId = HttpContext.Session.GetInt32("MyID");
            if (!myId.HasValue || myId.Value == 0)
            {
                return RedirectToAction("LoginDb", "Account", new { timeout = true });
            }

            // Force requestor from session (NOT editable)
            model.RequestorIDNum = myId.Value;

            // Enforce basic location rules
            if (string.Equals(model.StorageLocation, "ShortTerm", StringComparison.OrdinalIgnoreCase))
            {
                model.TargetDuration ??= 180;
                // optional: clear long term reason when short term
                model.LongTermReason = null;
            }
            else if (string.Equals(model.StorageLocation, "LongTerm", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(model.LongTermReason))
                {
                    ModelState.AddModelError(nameof(model.LongTermReason),
                        "Long Term Reason is required when Storage Location is Long Term.");
                }
            }
            else
            {
                ModelState.AddModelError(nameof(model.StorageLocation), "Storage Location must be ShortTerm or LongTerm.");
            }

            if (!ModelState.IsValid)
            {
                // Preserve return parameters
                ViewBag.ReturnPage = returnPage ?? 1;
                ViewBag.ReturnPageSize = returnPageSize ?? 10;
                ViewBag.ReturnSortOrder = returnSortOrder ?? "InventoryRecid";
                ViewBag.ReturnSortDir = returnSortDir ?? "asc";

                // Requestor display info
                ViewBag.CurrentUserId = myId.Value;
                ViewBag.CurrentUserFullName = HttpContext.Session.GetString("FullName") ?? "";

                await PopulateShortCreateDropdowns(selectedOwnerId: model.OwnerIDNum);

                return View("~/Views/ShortTerm/ShortCreate.cshtml", model);
            }

            // Call spADDNewItem (existing in DB)
            // Note: use ExecuteSqlRawAsync because this SP performs inserts and returns no projection.
            var sql = @"
EXEC dbo.spADDNewItem
    @PartNumber,
    @PartDescription,
    @Model_Variant,
    @RevLevel,
    @OwnerIDNum,
    @ProgramName,
    @CustomerRecID,
    @UM,
    @TargetDuration,
    @ClassificationID,
    @SerialNumber,
    @UUTNumber,
    @GeneralComment,
    @ProgramPhaseID,
    @StorageLocation,
    @LongTermReason,
    @LogStatus,
    @BinNum,
    @ShelfRecid,
    @RequestDate,
    @QtyIn,
    @QtyOut,
    @locationHistory,
    @CommentsStored,
    @RequestorIDNum,
    @WONum,
    @RequestFormType,
    @ItemStatusID,
    @PickUpLocation,
    @OilCheck";

            var parameters = new[]
            {
                new SqlParameter("@PartNumber", (object?)model.PartNumber ?? DBNull.Value),
                new SqlParameter("@PartDescription", (object?)model.PartDescription ?? DBNull.Value),
                new SqlParameter("@Model_Variant", (object?)model.Model_Variant ?? DBNull.Value),
                new SqlParameter("@RevLevel", (object?)model.RevLevel ?? DBNull.Value),
                new SqlParameter("@OwnerIDNum", (object?)model.OwnerIDNum ?? DBNull.Value),
                new SqlParameter("@ProgramName", (object?)model.ProgramName ?? DBNull.Value),
                new SqlParameter("@CustomerRecID", (object?)model.CustomerRecID ?? DBNull.Value),
                new SqlParameter("@UM", (object?)model.UM ?? DBNull.Value),
                new SqlParameter("@TargetDuration", (object?)model.TargetDuration ?? DBNull.Value),
                new SqlParameter("@ClassificationID", (object?)model.ClassificationID ?? DBNull.Value),
                new SqlParameter("@SerialNumber", (object?)model.SerialNumber ?? DBNull.Value),
                new SqlParameter("@UUTNumber", (object?)model.UUTNumber ?? DBNull.Value),
                new SqlParameter("@GeneralComment", (object?)model.GeneralComment ?? DBNull.Value),
                new SqlParameter("@ProgramPhaseID", (object?)model.ProgramPhaseID ?? DBNull.Value),
                new SqlParameter("@StorageLocation", (object?)model.StorageLocation ?? DBNull.Value),
                new SqlParameter("@LongTermReason", (object?)model.LongTermReason ?? DBNull.Value),
                new SqlParameter("@LogStatus", (object?)model.LogStatus ?? DBNull.Value),

                new SqlParameter("@BinNum", (object?)model.BinNum ?? DBNull.Value),
                new SqlParameter("@ShelfRecid", (object?)model.ShelfRecid ?? DBNull.Value),
                new SqlParameter("@RequestDate", (object?)model.RequestDate ?? DBNull.Value),
                new SqlParameter("@QtyIn", (object?)model.QtyIn ?? DBNull.Value),
                new SqlParameter("@QtyOut", (object?)model.QtyOut ?? DBNull.Value),

                // Note: SP param is @locationHistory in your script; SQL Server is usually case-insensitive,
                // but we keep the same casing here.
                new SqlParameter("@locationHistory", (object?)model.LocationHistory ?? DBNull.Value),

                new SqlParameter("@CommentsStored", (object?)model.CommentsStored ?? DBNull.Value),
                new SqlParameter("@RequestorIDNum", (object?)model.RequestorIDNum ?? DBNull.Value),
                new SqlParameter("@WONum", (object?)model.WONum ?? DBNull.Value),
                new SqlParameter("@RequestFormType", (object?)model.RequestFormType ?? DBNull.Value),
                new SqlParameter("@ItemStatusID", (object?)model.ItemStatusID ?? DBNull.Value),
                new SqlParameter("@PickUpLocation", (object?)model.PickUpLocation ?? DBNull.Value),
                new SqlParameter("@OilCheck", (object?)model.OilCheck ?? DBNull.Value),
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);

            // Back to list (preserve paging/sort if caller used them)
            return RedirectToAction("ShortIndex", new
            {
                page = returnPage ?? 1,
                pageSize = returnPageSize ?? 10,
                sortOrder = returnSortOrder ?? "InventoryRecid",
                sortDir = returnSortDir ?? "asc"
            });
        }

        // Helper: dropdown lists for ShortCreate (from existing EF tables)
        private async Task PopulateShortCreateDropdowns(int? selectedOwnerId)
        {
            // Owners (Users)
            ViewBag.Owners = await _context.InventoryUsers
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new SelectListItem
                {
                    Value = u.MyID.ToString(),
                    Text = (u.LastName + ", " + u.FirstName).Trim()
                })
                .ToListAsync();

            // Customers
            ViewBag.Customers = await _context.InventoryCustomer
                .OrderBy(c => c.CustomerName)
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerRecID.ToString(),
                    Text = c.CustomerName
                })
                .ToListAsync();

            // Classifications
            ViewBag.Classifications = await _context.InventoryClassification
                .OrderBy(c => c.Classification)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassificationID.ToString(),
                    Text = c.Classification
                })
                .ToListAsync();

            // Program Phases
            ViewBag.ProgramPhases = await _context.InventoryProjectPhase
                .OrderBy(p => p.PhaseName)
                .Select(p => new SelectListItem
                {
                    Value = p.ProgramPhaseID.ToString(),
                    Text = p.PhaseName
                })
                .ToListAsync();

            // Item Status
            ViewBag.ItemStatuses = await _context.InventoryItemStatus
                .OrderBy(s => s.ItemStatus)
                .Select(s => new SelectListItem
                {
                    Value = s.ItemStatusID.ToString(),
                    Text = s.ItemStatus
                })
                .ToListAsync();

            // Shelves (optional)
            ViewBag.Shelves = await _context.InventoryShelf
                .OrderBy(s => s.ShelfName)
                .Select(s => new SelectListItem
                {
                    Value = s.ShelfRecid.ToString(),
                    Text = s.ShelfName
                })
                .ToListAsync();
        }












        // __________________________________________________________________

        // GET: invShortTerm/Edit/5
        public async Task<IActionResult> ShortEdit(
            int id,
            int? returnPage = null,
            int? returnPageSize = null,
            string? returnSortOrder = null,
            string? returnSortDir = null)
          {
            // This section gets the record from the stored procedure.
            var param = new SqlParameter("@InventoryRecid", id);

            var rows = await _context.InvShortTermEdit
                .FromSqlRaw("EXEC dbo.spGETShortTermById @InventoryRecid", param)
                .AsNoTracking()
                .ToListAsync();

            var model = rows.FirstOrDefault();
            if (model == null) return NotFound();
            // Load Classifications for the dropdown

            var classifications = await _context.InventoryClassification
                .OrderBy(c => c.Classification)
                .ToListAsync();

            await LoadDropdownOptions(model);
            // Store return parameters in ViewBag for the view
            ViewBag.ReturnPage = returnPage ?? 1;
            ViewBag.ReturnPageSize = returnPageSize ?? 10;
            ViewBag.ReturnSortOrder = returnSortOrder ?? "InventoryRecid";
            ViewBag.ReturnSortDir = returnSortDir ?? "asc";
            return View(model);
        }

        // POST: ShortTerm/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShortEdit(
            InvShortTermEditModel model,
            int? returnPage = null,
            int? returnPageSize = null,
            string? returnSortOrder = null,
            string? returnSortDir = null)
        {
            // basic sanity checks
            if (model == null || model.InventoryRecid == 0)
            {
                ModelState.AddModelError(string.Empty, "Missing record identifier.");

                // Reload all drop down boxes for the updated view
                await LoadDropdownOptions(model);

                // Preserve return parameters
                ViewBag.ReturnPage = returnPage ?? 1;
                ViewBag.ReturnPageSize = returnPageSize ?? 10;
                ViewBag.ReturnSortOrder = returnSortOrder ?? "InventoryRecid";
                ViewBag.ReturnSortDir = returnSortDir ?? "asc";

                return View(model);

            }

            if (!ModelState.IsValid)
            {
                // Reload all drop down boxes for the updated view
                await LoadDropdownOptions(model);

                // Preserve return parameters
                ViewBag.ReturnPage = returnPage ?? 1;
                ViewBag.ReturnPageSize = returnPageSize ?? 10;
                ViewBag.ReturnSortOrder = returnSortOrder ?? "InventoryRecid";
                ViewBag.ReturnSortDir = returnSortDir ?? "asc";

                return View(model);
            }

            try
            {
                // NOTE: stored procedure name and parameter list must match your DB.
                const string sp = "EXEC dbo.spUPDATEItem " +
                                  "@InventoryRecid, @PartNumber, @PartDescription, @TargetDuration, " +
                                  "@Model_Variant, @RevLevel, @ProgramName, @UM, @SerialNumber, @UUTNumber, " +
                                  "@FirstDateIn, @GeneralComment, @ClassificationID";

                var parameters = new[]
                {
                    new SqlParameter("@InventoryRecid", SqlDbType.Int) { Value = model.InventoryRecid },
                    new SqlParameter("@PartNumber", SqlDbType.NVarChar, 100) { Value = (object)model.PartNumber ?? DBNull.Value },
                    new SqlParameter("@PartDescription", SqlDbType.NVarChar, 500) { Value = (object)model.PartDescription ?? DBNull.Value },
                    new SqlParameter("@TargetDuration", SqlDbType.Int) { Value = (object)model.TargetDuration ?? DBNull.Value },
                    new SqlParameter("@Model_Variant", SqlDbType.NVarChar, 100) { Value = (object)model.Model_Variant ?? DBNull.Value },
                    new SqlParameter("@RevLevel", SqlDbType.NVarChar, 50) { Value = (object)model.RevLevel ?? DBNull.Value },
                    new SqlParameter("@ProgramName", SqlDbType.NVarChar, 100) { Value = (object)model.ProgramName ?? DBNull.Value },
                    new SqlParameter("@UM", SqlDbType.NVarChar, 10) { Value = (object)model.UM ?? DBNull.Value },
                    new SqlParameter("@SerialNumber", SqlDbType.NVarChar, 100) { Value = (object)model.SerialNumber ?? DBNull.Value },
                    new SqlParameter("@UUTNumber", SqlDbType.NVarChar, 100) { Value = (object)model.UUTNumber ?? DBNull.Value },
                    new SqlParameter("@FirstDateIn", SqlDbType.DateTime2) { Value = (object)model.FirstDateIn ?? DBNull.Value },
                    new SqlParameter("@GeneralComment", SqlDbType.NVarChar, -1) { Value = (object)model.GeneralComment ?? DBNull.Value },
                    new SqlParameter("@ClassificationID", SqlDbType.Int) { Value = model.ClassificationID }
                };

                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"EXEC dbo.spUPDATEItem
                    @InventoryRecid = {model.InventoryRecid},
                    @PartNumber = {model.PartNumber},
                    @PartDescription = {model.PartDescription},
                    @TargetDuration = {model.TargetDuration},
                    @Model_Variant = {model.Model_Variant},
                    @RevLevel = {model.RevLevel},
                    @ProgramName = {model.ProgramName},
                    @UM = {model.UM},
                    @SerialNumber = {model.SerialNumber},
                    @UUTNumber = {model.UUTNumber},
                    @FirstDateIn = {model.FirstDateIn},
                    @GeneralComment = {model.GeneralComment},
                    @ClassificationID = {model.ClassificationID},
                    @CustomerRecID = {model.CustomerRecID},
                    @ProgramPhaseID = {model.ProgramPhaseID},
                    @OwnerIDNum = {model.OwnerIDNum}");

                // reload the record from the DB via the keyless projection to reflect any DB-side changes
                // FIX: Add ToListAsync() first, then get FirstOrDefault
                var rows = await _context.InvShortTermEdit
                    .FromSqlInterpolated($"EXEC dbo.spGETShortTermById @InventoryRecid={model.InventoryRecid}")
                    .AsNoTracking()
                    .ToListAsync();  // ✅ Changed: Get all results first
                var updated = rows.FirstOrDefault();  // ✅ Then get first item in memory  

                if (updated == null)
                {
                    return NotFound();
                }
                // Reload all drop down boxes for the updated view
                await LoadDropdownOptions(updated);
               
                // Preserve return parameters
                ViewBag.ReturnPage = returnPage ?? 1;
                ViewBag.ReturnPageSize = returnPageSize ?? 10;
                ViewBag.ReturnSortOrder = returnSortOrder ?? "InventoryRecid";
                ViewBag.ReturnSortDir = returnSortDir ?? "asc";

                // return the DB-populated model to the view
                // Set TempData AND ViewBag (as backup)
                //TempData["Success"] = "Record Updated";
                ViewBag.SuccessMessage = "Record Updated";
                return View(updated);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes to the database.");
                await LoadDropdownOptions(model);
                // Preserve return parameters
                ViewBag.ReturnPage = returnPage ?? 1;
                ViewBag.ReturnPageSize = returnPageSize ?? 10;
                ViewBag.ReturnSortOrder = returnSortOrder ?? "InventoryRecid";
                ViewBag.ReturnSortDir = returnSortDir ?? "asc";

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred. {ex.Message}");
                await LoadDropdownOptions(model);
                // Preserve return parameters
                ViewBag.ReturnPage = returnPage ?? 1;
                ViewBag.ReturnPageSize = returnPageSize ?? 10;
                ViewBag.ReturnSortOrder = returnSortOrder ?? "InventoryRecid";
                ViewBag.ReturnSortDir = returnSortDir ?? "asc";
                return View(model);
            }
        }

        // Helper method to load all dropdown options
        private async Task LoadDropdownOptions(InvShortTermEditModel model)
        {
            // Load Classifications
            var classifications = await _context.InventoryClassification
                .OrderBy(c => c.Classification)
                .ToListAsync();

            model.ClassificationOptions = new SelectList(
                classifications,
                "ClassificationID",      // Value field
                "Classification",        // Display field
                model.ClassificationID   // Selected value
            );

            // Load Customers
            var customers = await _context.InventoryCustomer
                .OrderBy(c => c.CustomerName)
                .ToListAsync();

            model.CustomerOptions = new SelectList(
                customers,
                "CustomerRecID",         // Value field
                "CustomerName",          // Display field
                model.CustomerRecID      // Selected value
            );

            // Load Program Phases
            var phases = await _context.InventoryProjectPhase
                .OrderBy(p => p.PhaseName)
                .ToListAsync();

            model.PhaseOptions = new SelectList(
                phases,
                "ProgramPhaseID",        // Value field
                "PhaseName",             // Display field
                model.ProgramPhaseID     // Selected value
            );

            // Load Owners (Users) - MyID from Users table maps to OwnerIDNum in InventoryMaster
            var owners = await _context.InventoryUsers
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            // Create a combined display name for owners
            var ownerList = owners.Select(o => new
            {
                MyID = o.MyID,  // Use MyID from Users table (will be saved as OwnerIDNum)
                FullName = $"{o.LastName}, {o.FirstName}".Trim()
            }).ToList();

            model.OwnerOptions = new SelectList(
                ownerList,
                "MyID",                  // Value field (MyID from Users table)
                "FullName",              // Display field (LastName, FirstName)
                model.OwnerIDNum         // Selected value (current OwnerIDNum from InventoryMaster)
            );
        }
    }
}

