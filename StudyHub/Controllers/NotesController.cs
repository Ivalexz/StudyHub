using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHub.Data;
using StudyHub.Models;
using StudyHub.Services;

namespace StudyHub.Controllers
{
    public class NotesController : Controller
    {
        private readonly StudyHubContext _context;
        private readonly RedisService _redisService;

        public NotesController(StudyHubContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }
        
        public async Task<IActionResult> Index(int? subject, int? course)
        {
    
            var notes = await _redisService.GetApprovedNotesAsync();

            if (subject.HasValue)
                notes = notes.Where(n => n.SubjectName != null && n.Id == subject.Value).ToList();

            if (course.HasValue)
                notes = notes.Where(n => n.Course == course.Value).ToList();

            notes = notes.OrderByDescending(n => n.CreatedAt).ToList();
            
            var subjects = await _context.Subjects.ToListAsync();
            
            var topAuthors = await _redisService.GetTopAuthorsAsync();

            ViewBag.SelectedSubjectId = subject;
            ViewBag.SelectedCourse = course;
            ViewBag.Subjects = subjects;
            ViewBag.TopAuthors = topAuthors;

            return View(notes);
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
            var userId = User.FindFirst("sub")?.Value; 
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

                await _redisService.ClearCacheAsync();
                
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
            
            await _redisService.ClearCacheAsync();
            
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
