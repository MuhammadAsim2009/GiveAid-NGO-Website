using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Models;

namespace Give_AID.Controllers
{
    // QueryController — lets logged-in users submit support queries and view admin replies
    public class QueryController : Controller
    {
        // Database context injected by ASP.NET Core DI
        private readonly Give_AIDContext _context;

        public QueryController(Give_AIDContext context)
        {
            _context = context;
        }

        // ── Helper: check if the user is logged in ──────────────────────────────
        // Returns true only when a valid UserId is stored in session
        private bool IsLoggedIn() =>
            HttpContext.Session.GetInt32("UserId").HasValue;

        // Returns the current user's ID from session (0 if not set)
        private int CurrentUserId() =>
            HttpContext.Session.GetInt32("UserId") ?? 0;

        // ── GET /Query/Index — list THIS user's queries only ────────────────────
        public async Task<IActionResult> Index()
        {
            // Redirect to login if not authenticated
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            int userId = CurrentUserId();

            // Fetch only the queries submitted by the logged-in user
            // Never show other users' queries (row-level security)
            var myQueries = await _context.Queries
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(myQueries);
        }

        // ── GET /Query/Create — show the query submission form ──────────────────
        public IActionResult Create()
        {
            // Must be logged in to submit a query
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // ── POST /Query/Create — save the submitted query ───────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken] // protect against cross-site request forgery
        public async Task<IActionResult> Create(string subject, string message)
        {
            // Must be logged in to submit
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            // Validate that subject and message are not empty
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Subject and Message are required.";
                return View();
            }

            // Create a new Queries record — UserId always comes from session, never from form
            var query = new Queries
            {
                UserId    = CurrentUserId(),
                Subject   = subject.Trim(),
                Message   = message.Trim(),
                Status    = "Open",         // all new queries start as Open
                CreatedAt = DateTime.Now
            };

            _context.Queries.Add(query);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your query has been submitted. We will get back to you shortly!";
            return RedirectToAction(nameof(Index));
        }

        // ── GET /Query/Details/{id} — show a single query and admin reply ───────
        public async Task<IActionResult> Details(int id)
        {
            // Must be logged in
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            // Find the query by ID
            var query = await _context.Queries.FindAsync(id);

            // If not found, return 404
            if (query == null)
                return NotFound();

            // Row-level security: a user can only see their OWN queries
            if (query.UserId != CurrentUserId())
            {
                TempData["Error"] = "You do not have permission to view this query.";
                return RedirectToAction(nameof(Index));
            }

            return View(query);
        }
    }
}

