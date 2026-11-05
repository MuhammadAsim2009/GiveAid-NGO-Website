using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID;
using Give_AID.Models;

namespace Give_AID.Controllers
{
    // Controller handling user registration, authentication, session management, and logout
    public class AuthController : Controller
    {
        private readonly Give_AIDContext _context;

        // Constructor injecting the database context
        public AuthController(Give_AIDContext context)
        {
            _context = context;
        }

        // GET: /Auth/Register
        // Displays the registration page for new users
        [HttpGet]
        public IActionResult Register()
        {
            // If already logged in, redirect to home page
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Auth/Register
        // Handles new user registration form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email address is already registered
                bool emailExists = await _context.Users
                    .AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "An account with this email address already exists.");
                    return View(model);
                }

                // Create new user entity with securely hashed password
                var user = new Users
                {
                    Name = model.Name.Trim(),
                    Email = model.Email.Trim().ToLower(),
                    Password = PasswordHelper.HashPassword(model.Password),
                    Role = "User", // Default role for public registrations
                    Phone = model.Phone?.Trim(),
                    Address = model.Address?.Trim(),
                    Occupation = model.Occupation?.Trim(),
                    Organization = model.Organization?.Trim()
                };

                // Save new user to the database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Store session details upon registration (auto-login)
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);

                TempData["SuccessMessage"] = "Welcome to GIVE-AID! Your account has been created successfully.";
                return RedirectToAction("Index", "Home");
            }

            // Return view with validation errors if model state is invalid
            return View(model);
        }

        // GET: /Auth/Login
        // Displays the login form
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // If already logged in, redirect based on role
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                string? role = HttpContext.Session.GetString("UserRole");
                if (role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Auth/Login
        // Validates credentials and initializes session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                // Verify user exists and password hash matches
                if (user != null && PasswordHelper.VerifyPassword(model.Password, user.Password))
                {
                    // Set session variables (same keys as LMS project)
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.Name);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    // Redirect to returnUrl if valid local URL
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Admin users go to Admin Dashboard; standard users go to Home
                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    TempData["SuccessMessage"] = $"Welcome back, {user.Name}!";
                    return RedirectToAction("Index", "Home");
                }

                // If authentication fails, show generic friendly error
                ModelState.AddModelError(string.Empty, "Invalid email address or password.");
            }

            return View(model);
        }

        // GET or POST: /Auth/Logout
        // Clears user session and redirects to Home
        public IActionResult Logout()
        {
            // Clear all stored session variables
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/AccessDenied
        // Displayed when an unprivileged user attempts to access an admin area
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

