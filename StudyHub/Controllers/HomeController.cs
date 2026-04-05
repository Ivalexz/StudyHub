using Microsoft.AspNetCore.Mvc;
using StudyHub.Services;
using StudyHub.ViewModels;

namespace StudyHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly RedisService _redisService;

        public HomeController(RedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<IActionResult> Index()
        {

            var notes = await _redisService.GetApprovedNotesAsync();
            var topAuthors = await _redisService.GetTopAuthorsAsync();
            var pendingCount = await _redisService.GetPendingCountAsync();


            var model = new HomeIndexViewModel
            {
                Notes = notes,
                TopAuthors = topAuthors,
                PendingCount = pendingCount,
                ApprovedCount = notes.Count,
                TotalNotes = notes.Count + pendingCount
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ClearCache()
        {
            await _redisService.ClearCacheAsync();
            return RedirectToAction("Index");
        }
    }
}


