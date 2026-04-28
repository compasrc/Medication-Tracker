using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using System.Linq;

namespace Medication_Tracker.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext context;

        public LoginController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost]
        public IActionResult Index(User user)
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View("Index");
            }

            var loginInput = user.Username.Trim();

            var dbUser = context.Users
              .AsNoTracking()
              .Where(u => u.Username == loginInput || u.Email == loginInput)
              .Select(u => new
              {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.FirstName,
                        u.Password,
                        u.PasswordHash
                    })
                .FirstOrDefault();

            if (dbUser == null)
            {
                ModelState.AddModelError("", "User not found. Please try again.");
                return View("Index");
            }

            bool passwordValid = false;

            if (!string.IsNullOrEmpty(dbUser.PasswordHash))
            {
                passwordValid = BCrypt.Net.BCrypt.Verify(user.Password, dbUser.PasswordHash);
            }
            else if (!string.IsNullOrEmpty(dbUser.Password))
            {
                passwordValid = dbUser.Password == user.Password;
            }

            if (!passwordValid)
            {
                ModelState.AddModelError("", "Invalid password. Please try again.");
                return View("Index");
            }

            HttpContext.Session.SetString("User", dbUser.Username ?? "User");
            HttpContext.Session.SetString("FirstName", dbUser.FirstName ?? dbUser.Username ?? "User");
            HttpContext.Session.SetInt32("UserId", dbUser.Id);

            return RedirectToAction("Index", "Home"); 
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
