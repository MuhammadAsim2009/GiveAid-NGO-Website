using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Models;
using System.Diagnostics;

namespace Give_AID.Controllers
{
    // Controller responsible for all public-facing pages of the GIVE-AID portal
    public class HomeController : Controller
    {
        private readonly Give_AIDContext _context;

        // Constructor injecting the database context
        public HomeController(Give_AIDContext context)
        {
            _context = context;
        }

        // GET: /Home/Index
        // Main landing page with hero banner, statistics, causes, programmes, and partners
        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            try
            {
                // Fetch active causes for the donation showcase
                viewModel.FeaturedCauses = await _context.Causes
                    .Where(c => c.IsActive)
                    .Take(6)
                    .ToListAsync();

                // Fetch upcoming welfare programmes with NGO details
                viewModel.UpcomingProgrammes = await _context.Programmes
                    .Include(p => p.Ngo)
                    .OrderBy(p => p.EventDate)
                    .Take(3)
                    .ToListAsync();

                // Fetch partners for the partner strip
                viewModel.Partners = await _context.Partners
                    .Take(6)
                    .ToListAsync();

                // Fetch associated NGOs
                viewModel.AssociatedNgos = await _context.Ngos
                    .Take(4)
                    .ToListAsync();

                // Calculate summary impact statistics
                viewModel.TotalDonationsCount = await _context.Donations.CountAsync();
                viewModel.TotalFundsRaised = await _context.Donations
                    .Where(d => d.Status == "Completed")
                    .SumAsync(d => (decimal?)d.Amount) ?? 0;
                viewModel.TotalMembersCount = await _context.Users.CountAsync(u => u.Role == "User");
                viewModel.TotalNgosCount = await _context.Ngos.CountAsync();
            }
            catch
            {
                // Graceful fallback if database tables are still being migrated
            }

            return View(viewModel);
        }

        // GET: /Home/About?slug=what-we-do
        // Displays About Us sub-pages (What we do, Mission, Team, Careers, Achievements, Supporters, Read About us)
        public async Task<IActionResult> About(string? slug = "what-we-do")
        {
            // Normalize slug
            slug = string.IsNullOrWhiteSpace(slug) ? "what-we-do" : slug.ToLower().Trim();

            // All valid About Us sub-pages for navigation menu
            var subPages = new Dictionary<string, string>
            {
                { "what-we-do", "What We Do" },
                { "our-mission", "Our Mission & Vision" },
                { "our-team", "Our Leadership Team" },
                { "career-with-us", "Career With Us" },
                { "our-achievements", "Our Achievements" },
                { "our-supporters", "Our Supporters" },
                { "read-about-us", "Read About Us" }
            };

            ViewBag.SubPages = subPages;
            ViewBag.CurrentSlug = slug;

            PageContents? content = null;
            try
            {
                content = await _context.PageContents
                    .FirstOrDefaultAsync(p => p.Slug.ToLower() == slug);
            }
            catch
            {
                // Table not yet migrated
            }

            // Fallback content if database record does not exist
            if (content == null)
            {
                string title = subPages.ContainsKey(slug) ? subPages[slug] : "About Us";
                content = new PageContents
                {
                    Slug = slug,
                    Title = title,
                    Body = $"Information regarding {title} is currently being updated by the administration. Please check back shortly."
                };
            }

            return View(content);
        }

        // GET: /Home/Programmes?category=Education
        // Lists welfare programmes with category filtering
        public async Task<IActionResult> Programmes(string? category)
        {
            var query = _context.Programmes.Include(p => p.Ngo).AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category.ToLower() == category.ToLower());
                ViewBag.SelectedCategory = category;
            }

            List<Programmes> programmes = new();
            try
            {
                programmes = await query.OrderBy(p => p.EventDate).ToListAsync();
            }
            catch
            {
                // Table not yet migrated
            }

            return View(programmes);
        }

        // GET: /Home/Partners
        // Displays partner companies and sponsor organizations
        public async Task<IActionResult> Partners()
        {
            List<Partners> partners = new();
            try
            {
                partners = await _context.Partners.ToListAsync();
            }
            catch
            {
                // Table not yet migrated
            }

            return View(partners);
        }

        // GET: /Home/Ngos
        // Displays list of associated NGOs and their welfare initiatives
        public async Task<IActionResult> Ngos()
        {
            List<Ngos> ngos = new();
            try
            {
                ngos = await _context.Ngos
                    .Include(n => n.Programmes)
                    .ToListAsync();
            }
            catch
            {
                // Table not yet migrated
            }

            return View(ngos);
        }

        // GET: /Home/Gallery
        // Displays photos from welfare programmes and charity distributions
        public async Task<IActionResult> Gallery(int? programmeId)
        {
            var query = _context.GalleryImages.Include(g => g.Programme).AsQueryable();

            if (programmeId.HasValue)
            {
                query = query.Where(g => g.ProgrammeId == programmeId.Value);
            }

            List<GalleryImages> images = new();
            try
            {
                images = await query.ToListAsync();
                ViewBag.Programmes = await _context.Programmes.ToListAsync();
            }
            catch
            {
                // Table not yet migrated
            }

            return View(images);
        }

        // GET: /Home/HelpCentre
        // Displays Help Centre FAQs and support information
        public async Task<IActionResult> HelpCentre()
        {
            PageContents? content = null;
            try
            {
                content = await _context.PageContents
                    .FirstOrDefaultAsync(p => p.Slug.ToLower() == "help-centre");
            }
            catch
            {
                // Table not yet migrated
            }

            if (content == null)
            {
                content = new PageContents
                {
                    Slug = "help-centre",
                    Title = "Help Centre & Support",
                    Body = "Welcome to the GIVE-AID Help Centre. Here you can find answers to commonly asked questions about donations, NGO verification, tax relief receipts, and volunteer opportunities."
                };
            }

            return View(content);
        }

        // GET: /Home/Contact
        // Displays contact info and secretariat details
        public IActionResult Contact()
        {
            return View();
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: /Home/Error/404 (or any status code)
        // Called by UseStatusCodePagesWithReExecute for HTTP error codes like 404
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error404()
        {
            // Return the friendly 404 page from Views/Shared/Error404.cshtml
            Response.StatusCode = 404;
            return View("~/Views/Shared/Error404.cshtml");
        }
    }
}
