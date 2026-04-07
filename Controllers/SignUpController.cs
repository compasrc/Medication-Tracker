using Microsoft.AspNetCore.Mvc;

namespace Medication_Tracker.Controllers
{
    public class SignUpController : Controller
     {
        public int ID {get; set;}
        public string Username {get; set;} = string.Empty;
        public string Password {get; set;} = string.Empty;

    }

    


}
