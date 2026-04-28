using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Diagnostics;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using System.Runtime.CompilerServices;


namespace Medication_Tracker.Controllers
{
    public class LoginController : Controller
    {
			private readonly ApplicationDbContext context; //database context for accessing the database
			
            public LoginController(ApplicationDbContext context)
			{
				this.context = context;
			}

            public IActionResult Index()
            {                
                return View("Index");
		    }
		    public IActionResult LoginDashBoard()
            {
                return View("Index");
            }

        

        [HttpPost]
        public IActionResult Login(User user)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View("Index");
            }

            var dbUser = context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (dbUser == null)
            {
                ModelState.AddModelError("", "User not found. Please try again.");
                return View("Index");
            }

            //SECURITY: bypass for current accounts, but need 
            if (string.IsNullOrEmpty(dbUser.PasswordHash))
            {
                return RedirectToAction("LoginDashBoard");
            }

			bool passwordValid = BCrypt.Net.BCrypt.Verify(user.Password, dbUser.PasswordHash);
            if (!passwordValid)
            {
                ModelState.AddModelError("", "Invalid password. Please try again.");
                return View("Index");
            }

            return RedirectToAction("Index", "Medication");
        }

            public IActionResult Logout()
            {
                return RedirectToAction("LoginDashBoard", "Login"); // Redirect to the login page
			}

            public IActionResult PasswordRecoveryControl(Medication User, string Email)
            {
                var user = context.Users.FirstOrDefault(u => u.Email == Email);
			    return RedirectToAction("PasswordRecovery", "PasswordRecovery");
			}

			public IActionResult NotSignedUp()
            {
                return RedirectToAction("SignUpUser", "SignUp");
            }
        }
}
