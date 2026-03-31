using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace StudyHub.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Login()
        {
            if (User.IsInRole("student"))
                return RedirectToAction("MyNotes", "Notes");
    
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Claims()
        {
            return Json(User.Claims.Select(c => new { c.Type, c.Value }));
        }
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Logout()
        {
            return SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                "Cookies", 
                "OpenIdConnect"
            );
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}