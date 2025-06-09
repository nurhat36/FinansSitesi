using FinansSitesi.Data;
using FinansSitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace FinansSitesi.Controllers
{
    [Authorize]
    public class NoteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NoteController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var notes = await _context.Notes
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Note note)
        {
            if (!ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                note.UserId = user.Id;
                note.CreatedAt = DateTime.Now;

                _context.Notes.Add(note);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(note);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null || note.UserId != _userManager.GetUserId(User))
                return NotFound();

            return View(note);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Note note)
        {
            if (!ModelState.IsValid)
            {
                var existing = await _context.Notes.FindAsync(note.Id);
                if (existing == null || existing.UserId != _userManager.GetUserId(User))
                    return NotFound();

                existing.Title = note.Title;
                existing.Content = note.Content;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(note);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null || note.UserId != _userManager.GetUserId(User))
                return NotFound();

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            return new ViewAsPdf("PdfView", note)
            {
                FileName = $"{note.Title}-Not.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }

    }
}
