using Medication_Tracker.Data;
using Medication_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using System.Linq;

namespace Medication_Tracker.Controllers
{
    public class SignUpController : Controller
    {
		private readonly ApplicationDbContext context; //database context for accessing the database
		public SignUpController(ApplicationDbContext context)
		{
			this.context = context;
		}

		public IActionResult SignUpUser()
        {
            var Account = new User();
            // Account.Name = placeholder until model for login info is created
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(User user)
        {
            if (ModelState.IsValid)
            {
               
                if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    Console.WriteLine($"You have successfully signed up. You will be redirected to the login page.");
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                   
                    ModelState.AddModelError(string.Empty, "Invalid sign-up attempt.");
                }
            }            
            return View();
        }
    }
}
