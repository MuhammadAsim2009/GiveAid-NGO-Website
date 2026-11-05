using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Give_AID.Models;

namespace Give_AID.Controllers
{
    // Controller handling the donation workflow: cause selection, dummy payment, and history
    public class DonationController : Controller
    {
        private readonly Give_AIDContext _context;

        // Constructor injecting the database context
        public DonationController(Give_AIDContext context)
        {
            _context = context;
        }

        // Helper method to get the currently logged-in user ID from session
        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        // ==========================================
        // MAKE A DONATION (Cause Selection + Payment)
        // ==========================================

        // GET: /Donation/Create?causeId=1&ngoId=2
        // Displays the donation form with cause and payment fields
        public async Task<IActionResult> Create(int? causeId, int? ngoId)
        {
            // Redirect to login if not authenticated
            if (GetUserId() == null)
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = "/Donation/Create" });
            }

            // Populate dropdown lists for cause and optional NGO selection
            ViewBag.Causes = new SelectList(
                await _context.Causes.Where(c => c.IsActive).ToListAsync(),
                "Id", "Name", causeId);

            ViewBag.Ngos = new SelectList(
                await _context.Ngos.ToListAsync(),
                "Id", "Name", ngoId);

            var model = new DonationViewModel
            {
                CauseId = causeId ?? 0,
                NgoId = ngoId
            };

            return View(model);
        }

        // POST: /Donation/Create
        // Processes the donation form with server-side card validation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonationViewModel model)
        {
            int? userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Server-side additional card validations beyond data annotations

            // Strip any spaces or dashes from card number for clean validation
            string cleanCard = model.CardNumber.Replace(" ", "").Replace("-", "");

            // Validate card number length (Visa/MC: 13-16 digits)
            if (!System.Text.RegularExpressions.Regex.IsMatch(cleanCard, @"^\d{13,16}$"))
            {
                ModelState.AddModelError("CardNumber", "Card number must contain 13 to 16 digits.");
            }
            else if (!PassesLuhnCheck(cleanCard))
            {
                // Validate card number using Luhn algorithm
                ModelState.AddModelError("CardNumber", "Card number is invalid. Please check and re-enter.");
            }

            // Validate expiry date is not in the past
            if (!string.IsNullOrEmpty(model.CardExpiry))
            {
                if (TryParseExpiry(model.CardExpiry, out int expMonth, out int expYear))
                {
                    var now = DateTime.UtcNow;
                    if (expYear < now.Year % 100 || (expYear == now.Year % 100 && expMonth < now.Month))
                    {
                        ModelState.AddModelError("CardExpiry", "Card has expired. Please use a valid card.");
                    }
                }
                else
                {
                    ModelState.AddModelError("CardExpiry", "Invalid expiry date format. Use MM/YY.");
                }
            }

            if (ModelState.IsValid)
            {
                // Build and save the donation record
                // Only the last 4 digits of the card are stored for audit
                var donation = new Donations
                {
                    UserId = userId.Value,
                    CauseId = model.CauseId,
                    NgoId = model.NgoId,
                    Amount = model.Amount,
                    CardLast4 = cleanCard.Substring(cleanCard.Length - 4),
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Thank you! Your donation of Rs. {model.Amount:N0} has been processed successfully. Receipt: #DON-{donation.Id:D5}";
                return RedirectToAction(nameof(History));
            }

            // If validation failed, reload dropdowns and re-display the form
            ViewBag.Causes = new SelectList(
                await _context.Causes.Where(c => c.IsActive).ToListAsync(),
                "Id", "Name", model.CauseId);

            ViewBag.Ngos = new SelectList(
                await _context.Ngos.ToListAsync(),
                "Id", "Name", model.NgoId);

            return View(model);
        }

        // GET: /Donation/History
        // Shows donation history for the logged-in donor
        public async Task<IActionResult> History()
        {
            int? userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = "/Donation/History" });
            }

            // Load only donations belonging to the currently logged-in user (row-level security)
            var donations = await _context.Donations
                .Include(d => d.Cause)
                .Include(d => d.Ngo)
                .Where(d => d.UserId == userId.Value)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(donations);
        }

        // ==========================================
        // LUHN ALGORITHM — Card Number Validation
        // ==========================================

        // Checks if a numeric string passes the Luhn checksum algorithm
        private static bool PassesLuhnCheck(string cardNumber)
        {
            int sum = 0;
            bool doubleDigit = false;

            // Process digits from right to left
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(cardNumber[i])) return false;

                int digit = cardNumber[i] - '0';

                if (doubleDigit)
                {
                    digit *= 2;
                    if (digit > 9) digit -= 9;
                }

                sum += digit;
                doubleDigit = !doubleDigit;
            }

            return sum % 10 == 0;
        }

        // Parses MM/YY expiry string into separate integer month and year
        private static bool TryParseExpiry(string expiry, out int month, out int year)
        {
            month = 0;
            year = 0;
            var cleaned = expiry.Replace("/", "").Trim();
            if (cleaned.Length != 4) return false;
            return int.TryParse(cleaned.Substring(0, 2), out month) &&
                   int.TryParse(cleaned.Substring(2, 2), out year);
        }
    }
}

