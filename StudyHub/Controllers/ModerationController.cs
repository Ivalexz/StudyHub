using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;
using StudyHub.Services;

[Authorize(Roles = "moderator")]
public class ModerationController : Controller
{
    private readonly StudyHubContext _context;
    private readonly RedisService _redisService;

    public ModerationController(StudyHubContext context, RedisService redisService)
    {
        _context = context;
        _redisService = redisService;
    }

    public async Task<IActionResult> Index()
    {
        var pendingNotes = await _context.Notes
            .Where(n => n.Status == NoteStatus.Pending)
            .ToListAsync();

        ViewBag.QueueCount = await _redisService.GetPendingCountAsync();
        return View(pendingNotes);
    }

    public async Task<IActionResult> Approve(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null) return NotFound();

        note.Status = NoteStatus.Approved;
        await _context.SaveChangesAsync();

        await _redisService.ClearCacheAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Reject(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null) return NotFound();

        note.Status = NoteStatus.Rejected;
        await _context.SaveChangesAsync();
        
        await _redisService.ClearCacheAsync();

        return RedirectToAction(nameof(Index));
    }
}