using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;

[Authorize(Roles = "moderator")]
public class StatisticsController : Controller
{
    private readonly StudyHubContext _context;

    public StatisticsController(StudyHubContext context)
    {
        _context = context;
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

        return View();
    }

    public IActionResult ClearCache()
    {
        return RedirectToAction(nameof(Index));
    }
}