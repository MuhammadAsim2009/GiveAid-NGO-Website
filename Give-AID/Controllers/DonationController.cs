using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Give_AID.Models;

namespace Give_AID.Controllers
{
    // Controller handling the donation workflow: multi-payment channel support (Card, EasyPaisa, JazzCash, Bank Transfer) and history
    public class DonationController : Controller
    {
        private readonly Give_AIDContext _context;
        private readonly IWebHostEnvironment _env;

        // Constructor injecting the database context and hosting environment for receipt uploads
        public DonationController(Give_AIDContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Helper method to get the currently logged-in user ID from session
        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        // Helper method to safely store uploaded receipts
        private async Task<string?> SaveUploadedFileAsync(IFormFile? file, string subFolder = "receipts")
        {
            if (file == null || file.Length == 0) return null;

            string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string targetFolder = Path.Combine(webRoot, subFolder);
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            string destinationPath = Path.Combine(targetFolder, uniqueFileName);

            using (var stream = new FileStream(destinationPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{subFolder}/{uniqueFileName}";
        }

        // ==========================================
        // MAKE A DONATION (Cause Selection + Payment)
        // ==========================================

        // GET: /Donation/Create?causeId=1&ngoId=2
        // Displays the donation form with cause and payment channel fields
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

            // Fetch live payment channel configuration for EasyPaisa / JazzCash / Bank
            var paymentSettings = await _context.PaymentSettings.FirstOrDefaultAsync() ?? new PaymentSettings();
            ViewBag.PaymentSettings = paymentSettings;

            var model = new DonationViewModel
            {
                CauseId = causeId ?? 0,
                NgoId = ngoId,
                PaymentMethod = "Card"
            };

            return View(model);
        }

        // POST: /Donation/Create
        // Processes the donation form with channel-specific validation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonationViewModel model)
        {
            int? userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            string? uploadedReceiptPath = null;
            string cleanCard = string.Empty;

            if (model.PaymentMethod == "Card")
            {
                // Clear manual transfer fields validation if any
                ModelState.Remove(nameof(model.TransactionRef));
                ModelState.Remove(nameof(model.ReceiptFile));

                if (string.IsNullOrWhiteSpace(model.CardName))
                {
                    ModelState.AddModelError("CardName", "Cardholder name is required.");
                }

                if (string.IsNullOrWhiteSpace(model.CardNumber))
                {
                    ModelState.AddModelError("CardNumber", "Card number is required.");
                }
                else
                {
                    cleanCard = model.CardNumber.Replace(" ", "").Replace("-", "");
                    if (!System.Text.RegularExpressions.Regex.IsMatch(cleanCard, @"^\d{13,16}$"))
                    {
                        ModelState.AddModelError("CardNumber", "Card number must contain 13 to 16 digits.");
                    }
                    else if (!PassesLuhnCheck(cleanCard))
                    {
                        ModelState.AddModelError("CardNumber", "Card number is invalid. Please check and re-enter.");
                    }
                }

                if (string.IsNullOrWhiteSpace(model.CardExpiry))
                {
                    ModelState.AddModelError("CardExpiry", "Card expiry date is required.");
                }
                else
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

                if (string.IsNullOrWhiteSpace(model.CardCvv))
                {
                    ModelState.AddModelError("CardCvv", "CVV security code is required.");
                }
            }
            else // EasyPaisa, JazzCash, or BankTransfer
            {
                // Clear card validation errors because card is not used
                ModelState.Remove(nameof(model.CardName));
                ModelState.Remove(nameof(model.CardNumber));
                ModelState.Remove(nameof(model.CardExpiry));
                ModelState.Remove(nameof(model.CardCvv));

                if (model.ReceiptFile == null || model.ReceiptFile.Length == 0)
                {
                    ModelState.AddModelError("ReceiptFile", "Please attach a screenshot or image of your payment receipt.");
                }
                else
                {
                    string ext = Path.GetExtension(model.ReceiptFile.FileName).ToLowerInvariant();
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("ReceiptFile", "Receipt must be an image (JPG, PNG, WEBP) or PDF file.");
                    }
                    else if (model.ReceiptFile.Length > 8 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ReceiptFile", "Receipt file size cannot exceed 8 MB.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                string status = "Completed";

                if (model.PaymentMethod != "Card")
                {
                    // Upload receipt file
                    uploadedReceiptPath = await SaveUploadedFileAsync(model.ReceiptFile, "receipts");
                    // Manual transfers require admin verification
                    status = "Pending";
                }

                var donation = new Donations
                {
                    UserId = userId.Value,
                    CauseId = model.CauseId,
                    NgoId = model.NgoId,
                    Amount = model.Amount,
                    PaymentMethod = model.PaymentMethod,
                    CardLast4 = model.PaymentMethod == "Card" && cleanCard.Length >= 4 ? cleanCard.Substring(cleanCard.Length - 4) : null,
                    TransactionRef = model.PaymentMethod != "Card" ? model.TransactionRef?.Trim() : null,
                    ReceiptPath = uploadedReceiptPath,
                    Status = status,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                if (status == "Completed")
                {
                    TempData["SuccessMessage"] = $"Thank you! Your donation of Rs. {model.Amount:N0} has been processed successfully. Receipt: #DON-{donation.Id:D5}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Thank you! Your donation proof (#DON-{donation.Id:D5}) has been submitted for review. It will be verified shortly by our team.";
                }

                return RedirectToAction(nameof(History));
            }

            // If validation failed, reload dropdowns and payment settings
            ViewBag.Causes = new SelectList(
                await _context.Causes.Where(c => c.IsActive).ToListAsync(),
                "Id", "Name", model.CauseId);

            ViewBag.Ngos = new SelectList(
                await _context.Ngos.ToListAsync(),
                "Id", "Name", model.NgoId);

            ViewBag.PaymentSettings = await _context.PaymentSettings.FirstOrDefaultAsync() ?? new PaymentSettings();

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

        private static bool PassesLuhnCheck(string cardNumber)
        {
            int sum = 0;
            bool doubleDigit = false;

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
