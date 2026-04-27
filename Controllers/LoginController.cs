using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Diagnostics;
using Medication_Tracker.Data;


namespace Medication_Tracker.Controllers
{
    public class LoginController : Controller
    {

        public class HomeController : Controller
        {
			private readonly ApplicationDbContext context; //database context for accessing the database
			public HomeController(ApplicationDbContext context)
			{
				this.context = context;
			}
			public IActionResult LoginDashBoard()
            {
                return View();
            }

			[HttpPost]
            public async Task<IActionResult> Login(User user)
            { 
				if (ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrWhiteSpace(user.PasswordHash))
                    {
                        return RedirectToAction("Dashboard");
                    }
                    else
                    {
                        // If the login fails, add an error message to the model state
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                return View();
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
}