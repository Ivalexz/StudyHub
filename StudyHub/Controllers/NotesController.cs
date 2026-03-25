using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;

namespace StudyHub.Controllers
{
    public class NotesController : Controller
    {
        private readonly StudyHubContext _context;

        public NotesController(StudyHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int id)
        {
            var note = await _context.Notes
                .Include(n => n.Subject)
                .FirstOrDefaultAsync(n => n.Id == id && n.Status == NoteStatus.Approved);

            if (note == null)
                return NotFound();

            return View(note);
        }
        
        [Authorize(Roles = "student")]
        public async Task<IActionResult> MyNotes()
        {
            var userId = User.FindFirst("sub")?.Value; // claim з Keycloak
            var notes = await _context.Notes
                .Where(n => n.AuthorId == userId)
                .Include(n => n.Subject)
                .ToListAsync();

            return View(notes);
        }

        [Authorize(Roles = "student")]
        public IActionResult Create()
        {
            ViewBag.Subjects = _context.Subjects.ToList();
            return View();
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Note note)
        {
            if (ModelState.IsValid)
            {
                note.AuthorId = User.FindFirst("sub")?.Value;
                note.AuthorName = User.Identity?.Name;
                note.Status = NoteStatus.Pending;
                note.CreatedAt = DateTime.UtcNow;

                _context.Add(note);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyNotes));
            }
            return View(note);
        }

        [Authorize(Roles = "student")]
        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.NoteId == id && l.UserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                var note = await _context.Notes.FindAsync(id);
                if (note != null) note.LikesCount--;
            }
            else
            {
                _context.Likes.Add(new Like { NoteId = id, UserId = userId });
                var note = await _context.Notes.FindAsync(id);
                if (note != null) note.LikesCount++;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
