using System;
using System.Linq;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace Medication_Tracker.Controllers
{
    public class SignUpController : Controller
    {
		private readonly ApplicationDbContext context;
		public SignUpController(ApplicationDbContext context)
		{
			this.context = context;
		}

        public IActionResult Index()
        {
            return View("Index");
		}

		public IActionResult SignUpUser()
        {
            var Account = new User();
            return View();
        }
        [HttpPost]
        public IActionResult SignUp(User user)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", user);
            }
              
            if (string.IsNullOrEmpty(user.Password))
            {                
                ModelState.AddModelError("Password", "Password is required.");
                return View("Index", user);
			}
			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);                  
			context.Users.Add(user);
            context.SaveChanges();
            TempData["SuccessMessage"] = "You have successfully signed up. You will be redirected to the login page.";
            return RedirectToAction("Index", "Login");
            }
                       
        }
    }
