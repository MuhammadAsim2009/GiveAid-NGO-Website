using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Models;
using Give_AID.Services;

namespace Give_AID.Controllers
{
    // InviteController — lets logged-in users invite friends to GIVE-AID via email
    public class InviteController : Controller
    {
        private readonly Give_AIDContext _context;
        private readonly IEmailService _emailService;

        public InviteController(Give_AIDContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ── Helper: check if the user is logged in ──────────────────────────────
        private bool IsLoggedIn() =>
            HttpContext.Session.GetInt32("UserId").HasValue;

        // Returns the current user's ID from session
        private int CurrentUserId() =>
            HttpContext.Session.GetInt32("UserId") ?? 0;

        // ── GET /Invite/Index — show invite form + list of invites sent ─────────
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            int userId = CurrentUserId();

            var invites = await _context.Invites
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.SentAt)
                .ToListAsync();

            return View(invites);
        }

        // ── POST /Invite/Send — record invite and try to send email ─────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string friendEmail)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(friendEmail))
            {
                TempData["Error"] = "Please enter a valid email address.";
                return RedirectToAction(nameof(Index));
            }

            friendEmail = friendEmail.Trim().ToLowerInvariant();

            int userId = CurrentUserId();

            // ── Save the invite record to the database ───────────────────────────
            var invite = new Invites
            {
                UserId      = userId,
                FriendEmail = friendEmail,
                SentAt      = DateTime.UtcNow
            };
            _context.Invites.Add(invite);
            await _context.SaveChangesAsync();

            // ── Dispatch the invitation email via EmailService ────────────────────
            try
            {
                string senderDisplayName = HttpContext.Session.GetString("UserName") ?? "A GIVE-AID Member";
                
                // Build dynamic registration URL
                string registerUrl = $"{Request.Scheme}://{Request.Host}/Auth/Register";

                await _emailService.SendInvitationEmailAsync(friendEmail, senderDisplayName, registerUrl);

                TempData["Success"] = $"Branded invitation dispatched to {friendEmail}!";
            }
            catch (Exception)
            {
                // Email sending failed — invitation is still saved in database
                TempData["Warning"] = $"Invitation recorded for {friendEmail}, but the email could not be delivered. Please verify SMTP configuration.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
