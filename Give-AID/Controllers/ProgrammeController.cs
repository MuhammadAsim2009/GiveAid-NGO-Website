using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Models;

namespace Give_AID.Controllers
{
    // ProgrammeController — lets users browse programme details and register their interest
    public class ProgrammeController : Controller
    {
        // Database context injected by ASP.NET Core DI
        private readonly Give_AIDContext _context;

        public ProgrammeController(Give_AIDContext context)
        {
            _context = context;
        }

        // ── Helper: check if the user is logged in ──────────────────────────────
        private bool IsLoggedIn() =>
            HttpContext.Session.GetInt32("UserId").HasValue;

        // Returns the current user's ID from session
        private int CurrentUserId() =>
            HttpContext.Session.GetInt32("UserId") ?? 0;

        // ── GET /Programme/Details/{id} — show programme info + interest form ───
        public async Task<IActionResult> Details(int id)
        {
            // Load the programme by ID, include related NGO data for display
            var programme = await _context.Programmes
                .Include(p => p.Ngo)
                .FirstOrDefaultAsync(p => p.Id == id);

            // Return 404 if programme not found
            if (programme == null)
                return NotFound();

            // If user is logged in, check whether they already registered interest
            bool alreadyRegistered = false;
            if (IsLoggedIn())
            {
                alreadyRegistered = await _context.ProgrammeInterests
                    .AnyAsync(pi => pi.ProgrammeId == id && pi.UserId == CurrentUserId());
            }

            // Pass flag to view so the form or a "Already Registered" message is shown
            ViewBag.AlreadyRegistered = alreadyRegistered;
            return View(programme);
        }

        // ── POST /Programme/RegisterInterest — save user's interest ─────────────
        [HttpPost]
        [ValidateAntiForgeryToken] // protect against CSRF attacks
        public async Task<IActionResult> RegisterInterest(int programmeId, string message)
        {
            // Must be logged in to register interest
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            int userId = CurrentUserId();

            // Prevent duplicate interest registration for the same programme
            bool alreadyRegistered = await _context.ProgrammeInterests
                .AnyAsync(pi => pi.ProgrammeId == programmeId && pi.UserId == userId);

            if (alreadyRegistered)
            {
                TempData["Error"] = "You have already registered interest in this programme.";
                return RedirectToAction(nameof(Details), new { id = programmeId });
            }

            // Save the interest record — UserId comes from session, not from form
            var interest = new ProgrammeInterests
            {
                UserId      = userId,
                ProgrammeId = programmeId,
                Message     = message?.Trim(),
                CreatedAt   = DateTime.UtcNow
            };

            _context.ProgrammeInterests.Add(interest);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your interest has been registered! We will contact you soon.";
            return RedirectToAction(nameof(MyInterests));
        }

        // ── GET /Programme/MyInterests — list all interests the user registered ─
        public async Task<IActionResult> MyInterests()
        {
            // Must be logged in to see interests
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            int userId = CurrentUserId();

            // Get interests for THIS user only — include programme name for display
            var interests = await _context.ProgrammeInterests
                .Include(pi => pi.Programme)
                .Where(pi => pi.UserId == userId)
                .OrderByDescending(pi => pi.CreatedAt)
                .ToListAsync();

            return View(interests);
        }
    }
}

