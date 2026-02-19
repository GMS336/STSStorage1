using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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

        //________________________________________________________________________________
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

                Console.WriteLine($"EditCheckOutLog GET: Loading data for CheckOutRecid={checkOutRecid}");

                // Fetch the specific checkout record using existing SP
                var checkOutRecidParam = new SqlParameter("@CheckOutRecid", checkOutRecid);
                var inventoryRecidParam = new SqlParameter("@InventoryRecid", (object)DBNull.Value);

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

                // Look up ItemStatusID from ItemStatus string (with whitespace handling)
                int? itemStatusId = null;

                if (!string.IsNullOrEmpty(item.ItemStatus))
                {
                    // Load all statuses and do the comparison in memory (not in SQL)
                    var allStatuses = await _context.InventoryItemStatus.ToListAsync();

                    var trimmedStatus = item.ItemStatus.Trim();

                    var statusRecord = allStatuses
                        .FirstOrDefault(s => s.ItemStatus != null &&
                                           s.ItemStatus.Trim().Equals(trimmedStatus, StringComparison.OrdinalIgnoreCase));

                    if (statusRecord != null)
                    {
                        itemStatusId = statusRecord.ItemStatusID;
                    }
                }

                // Map InvCheckOutModel to InvCheckOutEditModel
                var editModel = new InvCheckOutEditModel
                {
                    CheckOutRecid = item.CheckOutRecid,
                    InventoryRecid = item.InventoryRecid,
                    RequestorIDNum = item.RequestorIDNum,
                    RequestorName = item.RequestorName,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    RequestDate = item.RequestDate,
                    RequestFormType = item.RequestFormType,
                    DateIn = item.DateIn,
                    QtyIn = item.QtyIn,
                    CommentsStored = item.CommentsStored,
                    DateOut = item.DateOut,
                    QtyOut = item.QtyOut,
                    CommentRetrieval = item.CommentRetrieval,
                    LocationHistory = item.LocationHistory,
                    ShelfRecid = item.ShelfRecid,
                    ShelfName = item.ShelfName,
                    BinNum = item.BinNum,
                    LTStorageNum = item.LTStorageNum,
                    ItemStatus = item.ItemStatus,
                    ItemStatusID = itemStatusId,  // THIS IS THE KEY LINE!
                    WONum = item.WONum,
                    OilCheck = item.OilCheck,
                    RunningBalance = item.RunningBalance,
                    Balance = item.RunningBalance
                };

                // Load dropdown options
                await LoadCheckOutDropdownOptions(editModel);

                Console.WriteLine($"EditModel created - ItemStatusID: {editModel.ItemStatusID}");

                ViewBag.CheckOutRecid = checkOutRecid;

                return PartialView("~/Views/CheckOut/EditCheckOutLog.cshtml", editModel);  // RETURN editModel NOT item!
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EditCheckOutLog: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return Content($"<div class='alert alert-danger'>Error loading checkout record: {ex.Message}<br/><br/>{ex.InnerException?.Message}</div>", "text/html");
            }
        }

        // POST: CheckOut/EditCheckOutLog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCheckOutLog(InvCheckOutEditModel model, int? ItemStatusID, int? Balance, DateTime? NeedDate)
        {
            try
            {
                // Use the Balance from the form if provided, otherwise calculate it
                int newBalance = Balance ?? ((model.QtyIn ?? 0) - (model.QtyOut ?? 0));

                // Use ItemStatusID from model or from form parameter
                int? itemStatusId = model.ItemStatusID ?? ItemStatusID;

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

                // Redirect back to CheckoutLog with no success message
                return RedirectToAction("CheckoutLog", new { inventoryRecid = model.InventoryRecid });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EditCheckOutLog POST: {ex.Message}");

                // On error, reload the form with dropdown options
                await LoadCheckOutDropdownOptions(model);
                ViewBag.CheckOutRecid = model.CheckOutRecid;
                ModelState.AddModelError("", $"Error updating record: {ex.Message}");

                return PartialView("~/Views/CheckOut/EditCheckOutLog.cshtml", model);
            }
        }

        /// <summary>
        /// Loads dropdown options for the CheckOut Edit form (matching ShortTerm pattern)
        /// </summary>
        private async Task LoadCheckOutDropdownOptions(InvCheckOutEditModel model)
        {
            try
            {
                Console.WriteLine("LoadCheckOutDropdownOptions: Starting...");

                // Load Shelves
                var shelves = await _context.InventoryShelf
                    .OrderBy(s => s.ShelfName)
                    .ToListAsync();

                model.ShelfOptions = new SelectList(
                    shelves,
                    "ShelfRecid",       // Value field
                    "ShelfName",        // Display field
                    model.ShelfRecid    // Selected value
                );

                Console.WriteLine($"Loaded {shelves.Count} shelves into SelectList");

                // Load Requestors (Users) - Format: "ID - FirstName LastName"
                var requestors = await _context.InventoryUsers
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();

                var requestorList = requestors.Select(r => new
                {
                    MyID = r.MyID,
                    FullName = r.MyID + " - " + r.FirstName + " " + r.LastName
                }).ToList();

                model.RequestorOptions = new SelectList(
                    requestorList,
                    "MyID",                 // Value field
                    "FullName",             // Display field
                    model.RequestorIDNum    // Selected value
                );

                Console.WriteLine($"Loaded {requestorList.Count} requestors into SelectList");

                // Load Item Statuses
                var itemStatuses = await _context.InventoryItemStatus
                    .OrderBy(s => s.ItemStatus)
                    .ToListAsync();

                model.ItemStatusOptions = new SelectList(
                    itemStatuses,
                    "ItemStatusID",         // Value field
                    "ItemStatus",           // Display field
                    model.ItemStatusID      // Selected value
                );

                Console.WriteLine($"Loaded {itemStatuses.Count} item statuses into SelectList");

                // Load Bin Numbers from stored procedure
                var binNumbers = new List<BinNumberOption>();
                int lastBinUsed = 0;

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "spADDNewBinNumber";
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    await _context.Database.OpenConnectionAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var binNum = reader.IsDBNull(reader.GetOrdinal("BinNum"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("BinNum"));

                            var lastBin = reader.IsDBNull(reader.GetOrdinal("LastBinUsed"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("LastBinUsed"));

                            if (binNum > 0)
                            {
                                binNumbers.Add(new BinNumberOption { BinNum = binNum });
                            }

                            if (lastBin > lastBinUsed)
                            {
                                lastBinUsed = lastBin;
                            }
                        }
                    }
                }

                // Add the "New" bin option at the top
                var newBinNum = lastBinUsed + 1;
                var binOptionsWithNew = new List<BinNumberOption>
                {
                    new BinNumberOption { BinNum = newBinNum, DisplayText = $"{newBinNum} (New)" }
                };
                binOptionsWithNew.AddRange(binNumbers.Select(b => new BinNumberOption
                {
                    BinNum = b.BinNum,
                    DisplayText = b.BinNum.ToString()
                }));

                model.BinOptions = new SelectList(
                    binOptionsWithNew,
                    "BinNum",           // Value field
                    "DisplayText",      // Display field
                    model.BinNum        // Selected value
                );

                Console.WriteLine($"Loaded {binOptionsWithNew.Count} bin numbers into SelectList (New bin: {newBinNum})");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadCheckOutDropdownOptions ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                // Ensure connection is closed
                if (_context.Database.GetDbConnection().State == System.Data.ConnectionState.Open)
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
        }

        // Helper class for bin number options
        private class BinNumberOption
        {
            public int BinNum { get; set; }
            public string DisplayText { get; set; } = string.Empty;
        }
    }
}