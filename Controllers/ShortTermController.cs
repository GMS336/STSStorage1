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

                // Reload classifications if returning to view
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
                // Reload classifications if returning to view
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


                //var param = new SqlParameter("@InventoryRecid", model.InventoryRecid);
                //var updated = await _context.InvShortTermEdit
                //    .FromSqlInterpolated($"EXEC dbo.spGETShortTermById @InventoryRecid={model.InventoryRecid}")
                //    .AsNoTracking()
                //    .FirstOrDefaultAsync();

                if (updated == null)
                {
                    return NotFound();
                }
                // Reload classifications for the updated view
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


//public async Task<IActionResult> ShortIndex(
//    string sortValue = "InventoryRecid", int page = 1, int pageSize = 10)
//{
//    // Run the stored procedure and get all results in memory
//    var allItems = await _context.InvShortTerm
//        .FromSqlRaw("EXEC spGETAllShortTerm @SortValue = {0}", sortValue)
//        .AsNoTracking()
//        .ToListAsync();

//    // Now do pagination in memory
//    var count = allItems.Count;
//    var items = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

//    var pagedList = new PaginatedList<InvShortTermModel>(items, count, page, pageSize);
//    ViewBag.CurrentPage = page;
//    ViewBag.PageSize = pageSize;
//    return View("~/Views/ShortTerm/ShortIndex.cshtml", pagedList);
//}
