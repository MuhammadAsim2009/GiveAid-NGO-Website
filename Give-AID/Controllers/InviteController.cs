using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Models;
using System.Net;
using System.Net.Mail;

namespace Give_AID.Controllers
{
    // InviteController — lets logged-in users invite friends to GIVE-AID via email
    public class InviteController : Controller
    {
        // Database context and configuration injected by ASP.NET Core DI
        private readonly Give_AIDContext _context;
        private readonly IConfiguration  _config;

        public InviteController(Give_AIDContext context, IConfiguration config)
        {
            _context = context;
            _config  = config;
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
            // Must be logged in to send invites
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            int userId = CurrentUserId();

            // Load all invites sent by this user (most recent first)
            var invites = await _context.Invites
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.SentAt)
                .ToListAsync();

            return View(invites);
        }

        // ── POST /Invite/Send — record invite and try to send email ─────────────
        [HttpPost]
        [ValidateAntiForgeryToken] // protect against CSRF attacks
        public async Task<IActionResult> Send(string friendEmail)
        {
            // Must be logged in to send an invite
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            // Validate that an email address was provided
            if (string.IsNullOrWhiteSpace(friendEmail))
            {
                TempData["Error"] = "Please enter a valid email address.";
                return RedirectToAction(nameof(Index));
            }

            friendEmail = friendEmail.Trim().ToLower();

            int userId = CurrentUserId();

            // ── Save the invite record to the database ───────────────────────────
            // We save it first so the history is recorded even if email fails
            var invite = new Invites
            {
                UserId      = userId,
                FriendEmail = friendEmail,
                SentAt      = DateTime.UtcNow
            };
            _context.Invites.Add(invite);
            await _context.SaveChangesAsync();

            // ── Try to send the invitation email via SMTP ────────────────────────
            // The SMTP credentials live in appsettings.json (never hard-coded)
            try
            {
                // Read SMTP settings from configuration
                string host        = _config["SmtpSettings:Host"]        ?? "smtp.gmail.com";
                int    port        = int.Parse(_config["SmtpSettings:Port"] ?? "587");
                bool   enableSsl   = bool.Parse(_config["SmtpSettings:EnableSsl"] ?? "true");
                string senderEmail = _config["SmtpSettings:SenderEmail"]  ?? "noreply@giveaid.org";
                string senderName  = _config["SmtpSettings:SenderName"]   ?? "GIVE-AID Welfare Portal";
                string smtpUser    = _config["SmtpSettings:Username"]     ?? "";
                string smtpPass    = _config["SmtpSettings:Password"]     ?? "";

                // Sender's name from session for a personalised message
                string senderDisplayName = HttpContext.Session.GetString("UserName") ?? "A GIVE-AID Member";

                // Build the email message
                using var mail   = new MailMessage();
                using var client = new SmtpClient(host, port);

                mail.From = new MailAddress(senderEmail, senderName);
                mail.To.Add(friendEmail);
                mail.Subject    = $"{senderDisplayName} invited you to GIVE-AID!";
                mail.IsBodyHtml = true;
                mail.Body       = $@"
                    <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;'>
                      <h2 style='color:#1E8E5A;'>You've been invited to GIVE-AID!</h2>
                      <p><strong>{senderDisplayName}</strong> thinks you would love GIVE-AID —
                         a platform where you can support amazing causes and make a real difference.</p>
                      <p>
                        <a href='https://giveaid.org/Auth/Register'
                           style='background:#1E8E5A;color:#fff;padding:12px 24px;
                                  border-radius:6px;text-decoration:none;font-weight:bold;'>
                          Join GIVE-AID Today
                        </a>
                      </p>
                      <p style='color:#888;font-size:12px;'>
                        If you did not expect this email, you can safely ignore it.
                      </p>
                    </div>";

                // Configure SMTP credentials if provided in settings
                client.EnableSsl = enableSsl;
                if (!string.IsNullOrEmpty(smtpUser))
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                await client.SendMailAsync(mail);

                TempData["Success"] = $"Invitation sent to {friendEmail}!";
            }
            catch (Exception)
            {
                // Email sending failed — invitation is still saved in database
                // Show a friendly message rather than crashing the app
                TempData["Warning"] = $"Invitation recorded for {friendEmail}, but the email could not be sent right now. Please check SMTP settings.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

