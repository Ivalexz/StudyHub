using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

[Authorize(Roles = "moderator")]
public class ModerationController : Controller
{
    private readonly StudyHubContext _context;

    public ModerationController(StudyHubContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var pendingNotes = await _context.Notes
            .Where(n => n.Status == NoteStatus.Pending)
            .ToListAsync();

        ViewBag.QueueCount = pendingNotes.Count;
        return View(pendingNotes);
    }

    public async Task<IActionResult> Approve(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null) return NotFound();

        note.Status = NoteStatus.Approved;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Reject(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null) return NotFound();

        note.Status = NoteStatus.Rejected;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}