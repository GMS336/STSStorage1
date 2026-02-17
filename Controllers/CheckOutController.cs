using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using STSStorage1.Data;
using STSStorage1.Models;
using System.Diagnostics;

namespace STSStorage1.Controllers
{
    public class CheckOutController : BaseController
    {
        private readonly STSStorage1Context _context;

        public CheckOutController(STSStorage1Context context)
        {
            _context = context;
        }

        // GET: CheckOut/CheckoutLog
        // Display all checkout/checkin history for a specific InventoryRecid
        public async Task<IActionResult> CheckoutLog(int inventoryRecid)
        {
            if (inventoryRecid <= 0)
            {
                return BadRequest("Invalid Inventory Record ID");
            }

            // Increase the command timeout to 4 minutes (240 seconds)
            _context.Database.SetCommandTimeout(240);

            var sw = new Stopwatch();
            sw.Start();

            // Execute stored procedure to get all checkout records for this inventory item
            var param = new SqlParameter("@InventoryRecid", inventoryRecid);

            var items = await _context.InvCheckOut
                .FromSqlRaw("EXEC spGETAllCheckOutItem @InventoryRecid", param)
                .AsNoTracking()
                .ToListAsync();

            sw.Stop();
            ViewBag.DataFetchTime = sw.ElapsedMilliseconds;
            ViewBag.ItemsCount = items.Count;
            ViewBag.InventoryRecid = inventoryRecid;

            // Calculate all computed fields
            CalculateComputedFields(items);

            // Calculate final quantity (running balance from last record)
            int finalQty = items.Any() ? items.Last().RunningBalance : 0;
            ViewBag.FinalQty = finalQty;

            // Get target duration from first record if available
            ViewBag.TargetDuration = items.FirstOrDefault()?.TargetDuration ?? 0;

            return View("~/Views/CheckOut/CheckoutLog.cshtml", items);
        }

        /// <summary>
        /// Calculates all computed fields for checkout log items
        /// </summary>
        private void CalculateComputedFields(List<InvCheckOutModel> items)
        {
            int runningBalance = 0;

            foreach (var item in items)
            {
                // Calculate Date Moved based on request type
                item.DateMoved = CalculateDateMoved(item);

                // Calculate Quantity Moved based on request type
                item.QtyMoved = CalculateQuantityMoved(item);

                // Calculate Running Balance
                int thisBalance = (item.QtyIn ?? 0) - (item.QtyOut ?? 0);
                runningBalance += thisBalance;
                item.RunningBalance = runningBalance;

                // Format Oil Check display
                item.OilCheckDisplay = string.IsNullOrEmpty(item.OilCheck) ? "No" : item.OilCheck;

                // Format Comments
                item.FormattedComments = FormatComments(item.CommentsStored, item.CommentRetrieval);
            }
        }

        /// <summary>
        /// Calculates the date moved based on request type
        /// </summary>
        private DateTime? CalculateDateMoved(InvCheckOutModel item)
        {
            if (item.RequestFormType == "Return")
            {
                return item.DateIn;
            }
            else
            {
                // Only return DateOut if it's a valid date (after year 2000)
                return item.DateOut != null && item.DateOut >= new DateTime(2000, 1, 1)
                    ? item.DateOut
                    : null;
            }
        }

        /// <summary>
        /// Calculates the quantity moved based on request type
        /// </summary>
        private int CalculateQuantityMoved(InvCheckOutModel item)
        {
            return item.RequestFormType == "Return"
                ? (item.QtyIn ?? 0)
                : (item.QtyOut ?? 0);
        }

        /// <summary>
        /// Formats comments by combining stored and retrieval comments with HTML line breaks
        /// </summary>
        private string FormatComments(string? commentsStored, string? commentRetrieval)
        {
            string stored = commentsStored?.Replace("\r\n", "<br>").Replace("\n", "<br>") ?? "";
            string retrieval = commentRetrieval?.Replace("\r\n", "<br>").Replace("\n", "<br>") ?? "";

            // Only add separator if both comments exist
            string separator = !string.IsNullOrEmpty(stored) && !string.IsNullOrEmpty(retrieval)
                ? "<br>"
                : "";

            return stored + separator + retrieval;
        }

        // TODO: Add these action methods later
        // GET: CheckOut/RetrieveItemRequest
        public IActionResult RetrieveItemRequest(int inventoryRecId, string requestFormType)
        {
            // Placeholder for retrieve item request form
            ViewBag.InventoryRecId = inventoryRecId;
            ViewBag.RequestFormType = requestFormType;
            return View();
        }

        // GET: CheckOut/ReturnItemRequest
        public IActionResult ReturnItemRequest(int inventoryRecId, string requestFormType)
        {
            // Placeholder for return item request form
            ViewBag.InventoryRecId = inventoryRecId;
            ViewBag.RequestFormType = requestFormType;
            return View();
        }

        // GET: CheckOut/EditCheckOutItem (OLD - keeping for now in case referenced elsewhere)
        public async Task<IActionResult> EditCheckOutItem(int inventoryRecid, int checkOutRecid)
        {
            // Placeholder for edit checkout item
            // You'll implement this later with the appropriate model and view
            ViewBag.InventoryRecid = inventoryRecid;
            ViewBag.CheckOutRecid = checkOutRecid;
            return View();
        }

        // ===== NEW: Edit CheckOut Log Entry =====

        // GET: CheckOut/EditCheckOutLog
        /// <summary>
        /// Displays the edit form for a specific checkout log entry in a modal
        /// </summary>
        public async Task<IActionResult> EditCheckOutLog(int checkOutRecid)
        {
            try
            {
                if (checkOutRecid <= 0)
                {
                    return BadRequest("Invalid CheckOut Record ID");
                }

                // Fetch the specific checkout record using existing SP
                var checkOutRecidParam = new SqlParameter("@CheckOutRecid", checkOutRecid);
                var inventoryRecidParam = new SqlParameter("@InventoryRecid", (object)DBNull.Value);

                // First get the list, then get the first item (can't use FirstOrDefaultAsync directly with FromSqlRaw)
                var items = await _context.InvCheckOut
                    .FromSqlRaw("EXEC spGETCheckOutItem @InventoryRecid, @CheckOutRecid",
                        inventoryRecidParam, checkOutRecidParam)
                    .AsNoTracking()
                    .ToListAsync();

                var item = items.FirstOrDefault();

                if (item == null)
                {
                    return Content("<div class='alert alert-danger'>CheckOut record not found</div>", "text/html");
                }

                // Load dropdown data for the form
                await LoadDropdownData();

                ViewBag.CheckOutRecid = checkOutRecid;

                return PartialView("~/Views/CheckOut/EditCheckOutLog.cshtml", item);
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error in EditCheckOutLog: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Return a simple error view for debugging
                return Content($"<div class='alert alert-danger'>Error loading checkout record: {ex.Message}<br/><br/>{ex.InnerException?.Message}</div>", "text/html");
            }
        }

        // POST: CheckOut/EditCheckOutLog
        /// <summary>
        /// Processes the edit form submission for a checkout log entry
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCheckOutLog(InvCheckOutModel model, int? ItemStatusID, int? Balance, DateTime? NeedDate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Use the Balance from the form if provided, otherwise calculate it
                    int newBalance = Balance ?? ((model.QtyIn ?? 0) - (model.QtyOut ?? 0));

                    // Look up ItemStatusID from ItemStatus string if not provided directly
                    int? itemStatusId = ItemStatusID;
                    if (itemStatusId == null && !string.IsNullOrEmpty(model.ItemStatus))
                    {
                        var statusRecord = await _context.InventoryItemStatus
                            .Where(s => s.ItemStatus == model.ItemStatus)
                            .FirstOrDefaultAsync();
                        itemStatusId = statusRecord?.ItemStatusID;
                    }

                    // Update the checkout record using existing SP with all parameters
                    var parameters = new[]
                    {
                        new SqlParameter("@CheckOutRecid", model.CheckOutRecid),
                        new SqlParameter("@InventoryRecid", model.InventoryRecid),
                        new SqlParameter("@RequestorIDNum", model.RequestorIDNum ?? (object)DBNull.Value),
                        new SqlParameter("@RequestDate", model.RequestDate ?? (object)DBNull.Value),
                        new SqlParameter("@NeedDate", NeedDate ?? (object)DBNull.Value),
                        new SqlParameter("@DateIn", model.DateIn ?? (object)DBNull.Value),
                        new SqlParameter("@QtyIn", model.QtyIn ?? (object)DBNull.Value),
                        new SqlParameter("@CommentsStored", model.CommentsStored ?? (object)DBNull.Value),
                        new SqlParameter("@DateOut", model.DateOut ?? (object)DBNull.Value),
                        new SqlParameter("@QtyOut", model.QtyOut ?? (object)DBNull.Value),
                        new SqlParameter("@RequestFormType", model.RequestFormType ?? (object)DBNull.Value),
                        new SqlParameter("@Balance", newBalance),
                        new SqlParameter("@CommentRetrieval", model.CommentRetrieval ?? (object)DBNull.Value),
                        new SqlParameter("@WONum", model.WONum ?? (object)DBNull.Value),
                        new SqlParameter("@OilCheck", model.OilCheck ?? (object)DBNull.Value),
                        new SqlParameter("@LocationHistory", model.LocationHistory ?? (object)DBNull.Value),
                        new SqlParameter("@ItemStatusID", itemStatusId ?? (object)DBNull.Value),
                        new SqlParameter("@LTSTorageNum", model.LTStorageNum ?? (object)DBNull.Value),
                        new SqlParameter("@ShelfRecid", model.ShelfRecid ?? (object)DBNull.Value),
                        new SqlParameter("@BinNum", model.BinNum ?? (object)DBNull.Value)
                    };

                    await _context.Database.ExecuteSqlRawAsync(
                        @"EXEC spUPDATECheckOutItem 
                            @CheckOutRecid, @InventoryRecid, @RequestorIDNum, @RequestDate, @NeedDate,
                            @DateIn, @QtyIn, @CommentsStored, @DateOut, @QtyOut, @RequestFormType,
                            @Balance, @CommentRetrieval, @WONum, @OilCheck, @LocationHistory,
                            @ItemStatusID, @LTSTorageNum, @ShelfRecid, @BinNum",
                        parameters);

                    TempData["SuccessMessage"] = "CheckOut record updated successfully!";
                    return RedirectToAction("CheckoutLog", new { inventoryRecid = model.InventoryRecid });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating record: {ex.Message}");
                }
            }

            // If we got this far, something failed, reload dropdown data and redisplay form
            await LoadDropdownData();
            ViewBag.CheckOutRecid = model.CheckOutRecid;
            return PartialView("~/Views/CheckOut/EditCheckOutLog.cshtml", model);
        }

        /// <summary>
        /// Loads dropdown data for the edit form (Requestors, Shelves, ItemStatuses, Bins)
        /// </summary>
        private async Task LoadDropdownData()
        {
            // Get all requestors (users) for the dropdown
            var requestors = await _context.InventoryUsers
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    u.MyID,
                    FullName = u.MyID + " - " + u.FirstName + " " + u.LastName
                })
                .ToListAsync();
            ViewBag.Requestors = requestors;

            // Get all shelves for the dropdown
            var shelves = await _context.InventoryShelf
                .OrderBy(s => s.ShelfName)
                .Select(s => new
                {
                    s.ShelfRecid,
                    s.ShelfName
                })
                .ToListAsync();
            ViewBag.Shelves = shelves;

            // Get all item statuses for the dropdown
            var itemStatuses = await _context.InventoryItemStatus
                .OrderBy(s => s.ItemStatus)
                .Select(s => new
                {
                    s.ItemStatusID,
                    s.ItemStatus
                })
                .ToListAsync();
            ViewBag.ItemStatuses = itemStatuses;

            // Get bin numbers using stored procedure
            try
            {
                var binNumbers = await _context.Database
                    .SqlQueryRaw<BinNumberResult>("EXEC spADDNewBinNumber")
                    .ToListAsync();

                var lastBinUsed = binNumbers.FirstOrDefault()?.LastBinUsed ?? 0;
                var newBinNum = lastBinUsed + 1;

                ViewBag.NewBinNum = newBinNum;
                ViewBag.AllBinNumbers = binNumbers.Select(b => b.BinNum).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading bin numbers: {ex.Message}");
                ViewBag.NewBinNum = 1;
                ViewBag.AllBinNumbers = new List<int>();
            }
        }

        /// <summary>
        /// Helper class for bin number stored procedure result
        /// </summary>
        private class BinNumberResult
        {
            public int BinNum { get; set; }
            public int LastBinUsed { get; set; }
        }
    }
}