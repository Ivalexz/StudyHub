using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Services;

[Authorize(Roles = "moderator")]
public class StatisticsController : Controller
{
    private readonly StudyHubContext _context;
    private readonly RedisService _redisService;

    public StatisticsController(StudyHubContext context, RedisService redisService)
    {
        _context = context;
        _redisService = redisService;
    }

    public async Task<IActionResult> Index()
    {
        var bySubjects = await _context.Notes
            .GroupBy(n => n.Subject.Name)
            .Select(g => new { Subject = g.Key, Count = g.Count() })
            .ToListAsync();

        var byCourses = await _context.Notes
            .GroupBy(n => n.Course)
            .Select(g => new { Course = g.Key, Count = g.Count() })
            .ToListAsync();

        var byStatus = await _context.Notes
            .GroupBy(n => n.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        ViewBag.BySubjects = bySubjects;
        ViewBag.ByCourses = byCourses;
        ViewBag.ByStatus = byStatus;

        var allNotes = await _context.Notes
            .Include(n => n.Subject)
            .ToListAsync();
        return View(allNotes);
    }

    public async Task<IActionResult> ClearCache()
    {
        await _redisService.ClearCacheAsync();
        return RedirectToAction(nameof(Index));
    }
}