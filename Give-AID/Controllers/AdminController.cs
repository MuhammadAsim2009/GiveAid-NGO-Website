using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Give_AID.Models;
using Give_AID.Services;

namespace Give_AID.Controllers
{
    // Controller managing all administrative functions, CMS content, and master records
    public class AdminController : Controller
    {
        private readonly Give_AIDContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;

        // Constructor injecting the database context, hosting environment, and email service
        public AdminController(Give_AIDContext context, IWebHostEnvironment env, IEmailService emailService)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
        }

        // Helper method to safely store uploaded files and return their public web path
        private async Task<string?> SaveUploadedFileAsync(IFormFile? file, string subFolder = "uploads")
        {
            if (file == null || file.Length == 0) return null;

            string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string targetFolder = Path.Combine(webRoot, subFolder);
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string extension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            string destinationPath = Path.Combine(targetFolder, uniqueFileName);

            using (var stream = new FileStream(destinationPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{subFolder}/{uniqueFileName}";
        }

        // Helper method to verify that an administrator session is active
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // ==========================================
        // 1. ADMIN DASHBOARD
        // ==========================================

        // GET: /Admin or /Admin/Index
        // Main overview dashboard displaying system statistics and recent entries
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path });
            }

            var dashboard = new AdminDashboardViewModel();

            try
            {
                dashboard.TotalMembersCount = await _context.Users.CountAsync(u => u.Role == "User");
                dashboard.TotalDonationsCount = await _context.Donations.CountAsync();
                dashboard.TotalFundsRaised = await _context.Donations
                    .Where(d => d.Status == "Completed")
                    .SumAsync(d => (decimal?)d.Amount) ?? 0;
                dashboard.OpenQueriesCount = await _context.Queries.CountAsync(q => q.Status == "Open");
                dashboard.TotalCausesCount = await _context.Causes.CountAsync();
                dashboard.TotalNgosCount = await _context.Ngos.CountAsync();
                dashboard.TotalProgrammesCount = await _context.Programmes.CountAsync();

                // Recent entries
                dashboard.RecentDonations = await _context.Donations
                    .Include(d => d.User)
                    .Include(d => d.Cause)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                dashboard.RecentQueries = await _context.Queries
                    .Include(q => q.User)
                    .OrderByDescending(q => q.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                dashboard.RecentMembers = await _context.Users
                    .Where(u => u.Role == "User")
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .ToListAsync();
            }
            catch
            {
                // Graceful fallback if migrations are pending
            }

            return View(dashboard);
        }

        // ==========================================
        // 2. CAUSES MANAGEMENT (CRUD)
        // ==========================================

        // GET: /Admin/Causes
        public async Task<IActionResult> Causes()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            var causes = await _context.Causes.Include(c => c.Donations).ToListAsync();
            return View(causes);
        }

        // GET: /Admin/CreateCause
        public IActionResult CreateCause()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            return View();
        }

        // POST: /Admin/CreateCause
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCause(Causes cause)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                _context.Causes.Add(cause);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cause created successfully!";
                return RedirectToAction(nameof(Causes));
            }
            return View(cause);
        }

        // GET: /Admin/EditCause/5
        public async Task<IActionResult> EditCause(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var cause = await _context.Causes.FindAsync(id);
            if (cause == null) return NotFound();

            return View(cause);
        }

        // POST: /Admin/EditCause/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCause(int id, Causes cause)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            if (id != cause.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(cause);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cause updated successfully!";
                return RedirectToAction(nameof(Causes));
            }
            return View(cause);
        }

        // POST: /Admin/DeleteCause/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCause(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var cause = await _context.Causes.Include(c => c.Donations).FirstOrDefaultAsync(c => c.Id == id);
            if (cause != null)
            {
                // Safety rule: if donations exist for this cause, deactivate instead of deleting to preserve history
                if (cause.Donations.Any())
                {
                    cause.IsActive = false;
                    _context.Update(cause);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cause has donations recorded; it has been deactivated instead of deleted.";
                }
                else
                {
                    _context.Causes.Remove(cause);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cause deleted successfully!";
                }
            }
            return RedirectToAction(nameof(Causes));
        }

        // ==========================================
        // 3. NGOS MANAGEMENT (CRUD)
        // ==========================================

        // GET: /Admin/Ngos
        public async Task<IActionResult> Ngos()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            var ngos = await _context.Ngos.Include(n => n.Programmes).ToListAsync();
            return View(ngos);
        }

        // GET: /Admin/CreateNgo
        public IActionResult CreateNgo()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            return View();
        }

        // POST: /Admin/CreateNgo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNgo(Ngos ngo, IFormFile? logoFile)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            if (logoFile != null && logoFile.Length > 0)
            {
                ngo.Logo = await SaveUploadedFileAsync(logoFile, "uploads/logos");
            }

            if (ModelState.IsValid)
            {
                _context.Ngos.Add(ngo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "NGO registered successfully!";
                return RedirectToAction(nameof(Ngos));
            }
            return View(ngo);
        }

        // GET: /Admin/EditNgo/5
        public async Task<IActionResult> EditNgo(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var ngo = await _context.Ngos.FindAsync(id);
            if (ngo == null) return NotFound();

            return View(ngo);
        }

        // POST: /Admin/EditNgo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNgo(int id, Ngos ngo, IFormFile? logoFile)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            if (id != ngo.Id) return NotFound();

            if (logoFile != null && logoFile.Length > 0)
            {
                ngo.Logo = await SaveUploadedFileAsync(logoFile, "uploads/logos");
            }

            if (ModelState.IsValid)
            {
                _context.Update(ngo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "NGO updated successfully!";
                return RedirectToAction(nameof(Ngos));
            }
            return View(ngo);
        }

        // POST: /Admin/DeleteNgo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNgo(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var ngo = await _context.Ngos.Include(n => n.Programmes).FirstOrDefaultAsync(n => n.Id == id);
            if (ngo != null)
            {
                if (ngo.Programmes.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete this NGO because it has active welfare programmes linked to it.";
                    return RedirectToAction(nameof(Ngos));
                }

                _context.Ngos.Remove(ngo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "NGO removed successfully!";
            }
            return RedirectToAction(nameof(Ngos));
        }

        // ==========================================
        // 4. PARTNERS MANAGEMENT (CRUD)
        // ==========================================

        // GET: /Admin/Partners
        public async Task<IActionResult> Partners()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            var partners = await _context.Partners.ToListAsync();
            return View(partners);
        }

        // GET: /Admin/CreatePartner
        public IActionResult CreatePartner()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            return View();
        }

        // POST: /Admin/CreatePartner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartner(Partners partner, IFormFile? logoFile)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            if (logoFile != null && logoFile.Length > 0)
            {
                partner.LogoPath = await SaveUploadedFileAsync(logoFile, "uploads/partners");
            }

            if (ModelState.IsValid)
            {
                _context.Partners.Add(partner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Corporate partner added successfully!";
                return RedirectToAction(nameof(Partners));
            }
            return View(partner);
        }

        // GET: /Admin/EditPartner/5
        public async Task<IActionResult> EditPartner(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var partner = await _context.Partners.FindAsync(id);
            if (partner == null) return NotFound();

            return View(partner);
        }

        // POST: /Admin/EditPartner/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartner(int id, Partners partner, IFormFile? logoFile)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            if (id != partner.Id) return NotFound();

            if (logoFile != null && logoFile.Length > 0)
            {
                partner.LogoPath = await SaveUploadedFileAsync(logoFile, "uploads/partners");
            }

            if (ModelState.IsValid)
            {
                _context.Update(partner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Partner details updated!";
                return RedirectToAction(nameof(Partners));
            }
            return View(partner);
        }

        // POST: /Admin/DeletePartner/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePartner(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var partner = await _context.Partners.FindAsync(id);
            if (partner != null)
            {
                _context.Partners.Remove(partner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Partner removed successfully!";
            }
            return RedirectToAction(nameof(Partners));
        }

        // ==========================================
        // 5. PROGRAMMES MANAGEMENT (CRUD)
        // ==========================================

        // GET: /Admin/Programmes
        public async Task<IActionResult> Programmes()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            var programmes = await _context.Programmes
                .Include(p => p.Ngo)
                .Include(p => p.ProgrammeInterests)
                .OrderByDescending(p => p.EventDate)
                .ToListAsync();
            return View(programmes);
        }

        // GET: /Admin/CreateProgramme
        public async Task<IActionResult> CreateProgramme()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            ViewBag.Ngos = new SelectList(await _context.Ngos.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: /Admin/CreateProgramme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProgramme(Programmes programme)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                _context.Programmes.Add(programme);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Welfare programme created successfully!";
                return RedirectToAction(nameof(Programmes));
            }

            ViewBag.Ngos = new SelectList(await _context.Ngos.ToListAsync(), "Id", "Name", programme.NgoId);
            return View(programme);
        }

        // GET: /Admin/EditProgramme/5
        public async Task<IActionResult> EditProgramme(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var programme = await _context.Programmes.FindAsync(id);
            if (programme == null) return NotFound();

            ViewBag.Ngos = new SelectList(await _context.Ngos.ToListAsync(), "Id", "Name", programme.NgoId);
            return View(programme);
        }

        // POST: /Admin/EditProgramme/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProgramme(int id, Programmes programme)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            if (id != programme.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(programme);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Programme details updated successfully!";
                return RedirectToAction(nameof(Programmes));
            }

            ViewBag.Ngos = new SelectList(await _context.Ngos.ToListAsync(), "Id", "Name", programme.NgoId);
            return View(programme);
        }

        // POST: /Admin/DeleteProgramme/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProgramme(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var programme = await _context.Programmes.FindAsync(id);
            if (programme != null)
            {
                _context.Programmes.Remove(programme);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Programme deleted successfully!";
            }
            return RedirectToAction(nameof(Programmes));
        }

        // ==========================================
        // 6. GALLERY MANAGEMENT
        // ==========================================

        // GET: /Admin/Gallery
        public async Task<IActionResult> Gallery()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            var images = await _context.GalleryImages.Include(g => g.Programme).ToListAsync();
            ViewBag.Programmes = new SelectList(await _context.Programmes.ToListAsync(), "Id", "Title");
            return View(images);
        }

        // POST: /Admin/CreateGalleryImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGalleryImage(GalleryImages image, IFormFile? imageFile)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            if (imageFile != null && imageFile.Length > 0)
            {
                image.ImagePath = await SaveUploadedFileAsync(imageFile, "uploads/gallery");
            }

            if (ModelState.IsValid)
            {
                _context.GalleryImages.Add(image);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Gallery image added successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add image. Please ensure title and image are provided.";
            }

            return RedirectToAction(nameof(Gallery));
        }

        // POST: /Admin/DeleteGalleryImage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGalleryImage(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var image = await _context.GalleryImages.FindAsync(id);
            if (image != null)
            {
                _context.GalleryImages.Remove(image);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Image removed from gallery!";
            }
            return RedirectToAction(nameof(Gallery));
        }

        // ==========================================
        // 7. PAGE CONTENTS (CMS)
        // ==========================================

        // GET: /Admin/Pages
        public async Task<IActionResult> Pages()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            var pages = await _context.PageContents.OrderBy(p => p.Title).ToListAsync();
            return View(pages);
        }

        // GET: /Admin/EditPage/5
        public async Task<IActionResult> EditPage(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var page = await _context.PageContents.FindAsync(id);
            if (page == null) return NotFound();

            return View(page);
        }

        // POST: /Admin/EditPage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPage(int id, PageContents page)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");
            if (id != page.Id) return NotFound();

            if (ModelState.IsValid)
            {
                page.UpdatedAt = DateTime.UtcNow;
                _context.Update(page);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Content for '{page.Title}' updated successfully!";
                return RedirectToAction(nameof(Pages));
            }
            return View(page);
        }

        // ==========================================
        // 8. MEMBERS & DONATIONS & QUERIES LISTS
        // ==========================================

        // GET: /Admin/Members
        public async Task<IActionResult> Members()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var members = await _context.Users
                .Include(u => u.Donations)
                .OrderByDescending(u => u.Id)
                .ToListAsync();

            return View(members);
        }

        // GET: /Admin/Donations
        public async Task<IActionResult> Donations()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var donations = await _context.Donations
                .Include(d => d.User)
                .Include(d => d.Cause)
                .Include(d => d.Ngo)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(donations);
        }

        // GET: /Admin/Interests
        public async Task<IActionResult> Interests()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var interests = await _context.ProgrammeInterests
                .Include(pi => pi.User)
                .Include(pi => pi.Programme)
                .OrderByDescending(pi => pi.CreatedAt)
                .ToListAsync();

            return View(interests);
        }

        // GET: /Admin/Queries
        public async Task<IActionResult> Queries()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var queries = await _context.Queries
                .Include(q => q.User)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(queries);
        }

        // POST: /Admin/ReplyQuery/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyQuery(int id, string reply, string status)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var query = await _context.Queries.Include(q => q.User).FirstOrDefaultAsync(q => q.Id == id);
            if (query != null)
            {
                query.Reply = reply?.Trim();
                query.Status = string.IsNullOrWhiteSpace(status) ? "Resolved" : status;
                _context.Update(query);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Query response saved successfully!";

                // Dispatch notification email to user
                if (query.User != null && !string.IsNullOrEmpty(query.User.Email))
                {
                    try
                    {
                        await _emailService.SendQueryReplyEmailAsync(
                            query.User.Email,
                            query.User.Name,
                            query.Subject,
                            query.Reply ?? "");
                    }
                    catch
                    {
                        // Email error is caught gracefully
                    }
                }
            }

            return RedirectToAction(nameof(Queries));
        }

        // ==========================================
        // PAYMENT CHANNELS & SETTINGS CONFIGURATION
        // ==========================================

        // GET: /Admin/PaymentSettings
        public async Task<IActionResult> PaymentSettings()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var settings = await _context.PaymentSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new PaymentSettings();
                _context.PaymentSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return View(settings);
        }

        // POST: /Admin/PaymentSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaymentSettings(PaymentSettings model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                var existing = await _context.PaymentSettings.FirstOrDefaultAsync(s => s.Id == model.Id);
                if (existing != null)
                {
                    existing.EasyPaisaNumber = model.EasyPaisaNumber?.Trim() ?? "";
                    existing.EasyPaisaTitle = model.EasyPaisaTitle?.Trim() ?? "";
                    existing.JazzCashNumber = model.JazzCashNumber?.Trim() ?? "";
                    existing.JazzCashTitle = model.JazzCashTitle?.Trim() ?? "";
                    existing.BankName = model.BankName?.Trim() ?? "";
                    existing.BankAccountTitle = model.BankAccountTitle?.Trim() ?? "";
                    existing.BankAccountNumber = model.BankAccountNumber?.Trim() ?? "";
                    existing.BankIBAN = model.BankIBAN?.Trim() ?? "";
                    existing.BankBranch = model.BankBranch?.Trim();
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existing);
                }
                else
                {
                    model.UpdatedAt = DateTime.UtcNow;
                    _context.PaymentSettings.Add(model);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Payment channel numbers and bank accounts updated successfully!";
                return RedirectToAction(nameof(PaymentSettings));
            }

            return View(model);
        }

        // POST: /Admin/UpdateDonationStatus
        // Approves or rejects a manual payment receipt submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDonationStatus(int id, string status)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Auth");

            var donation = await _context.Donations
                .Include(d => d.User)
                .Include(d => d.Cause)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donation != null)
            {
                donation.Status = status;
                _context.Update(donation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Donation #DON-{donation.Id:D5} status updated to {status}.";

                // If donation is approved, dispatch official donation receipt email
                if (status == "Completed" && donation.User != null && !string.IsNullOrEmpty(donation.User.Email))
                {
                    try
                    {
                        await _emailService.SendDonationReceiptEmailAsync(
                            donation.User.Email,
                            donation.User.Name,
                            donation.Amount,
                            $"DON-{donation.Id:D5}",
                            donation.Cause?.Name ?? "General Welfare Fund",
                            donation.PaymentMethod);
                    }
                    catch
                    {
                        // Email dispatch is non-blocking
                    }
                }
            }

            return RedirectToAction(nameof(Donations));
        }
    }
}

