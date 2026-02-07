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

        // GET: CheckOut/EditCheckOutItem
        public async Task<IActionResult> EditCheckOutItem(int inventoryRecid, int checkOutRecid)
        {
            // Placeholder for edit checkout item
            // You'll implement this later with the appropriate model and view
            ViewBag.InventoryRecid = inventoryRecid;
            ViewBag.CheckOutRecid = checkOutRecid;
            return View();
        }
    }
}