using Medication_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Medication_Tracker.Controllers
{
    public class LoginController : Controller
    {
        public int ID {get; set;}
        public string Username {get; set;} = string.Empty;
        public string Password {get; set;} = string.Empty;
        
       public class HomeController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginController model)
        {
            if (ModelState.IsValid)
            {
                // Here you would typically check the username and password against a database
                // For demonstration purposes, we'll just check if they are not empty
                if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.Password))
                {
                    // If the login is successful, redirect to a different page (e.g., dashboard)
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    // If the login fails, add an error message to the model state
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            // If we got this far, something failed; redisplay the form
            return View(model);
        }
    }
}
}