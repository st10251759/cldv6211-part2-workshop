using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediBook.Data;
using MediBook.Models;
using MediBook.Services;

/*
==============================Code Attribution==================================
ASP.NET MVC Controllers with Azure Blob Storage
Author: Microsoft
Link: [https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions)
Date Accessed: 28 April 2026
==============================Code Attribution==================================
*/

namespace MediBook.Controllers
{
    // Handles all CRUD operations for MedicalSession records.
    // Image uploads are handled via BlobService, which stores images in Azurite.
    // SessionCategory is nullable to support existing records already in the database.
    public class MedicalSessionsController : Controller
    {
        private readonly MediBookDbContext _context;
        private readonly BlobService _blobService;

        public MedicalSessionsController(MediBookDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: MedicalSessions — retrieves and displays all medical sessions
        public async Task<IActionResult> Index()
        {
            return View(await _context.MedicalSessions.ToListAsync());
        }

        // GET: MedicalSessions/Details/5 — displays full details of a single session
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.MedicalSessions
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            return View(session);
        }

        // GET: MedicalSessions/Create — returns the blank create form
        public IActionResult Create()
        {
            return View();
        }

        // POST: MedicalSessions/Create — uploads image to Azurite, saves session to database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("SessionId,Name,Description,StartDate,EndDate,Category,ImageFile")] MedicalSession session)
        {
            // Validate that StartDate is before EndDate
            if (session.StartDate >= session.EndDate)
                ModelState.AddModelError("EndDate", "End date must be after the start date.");

            // Remove ImageUrl from validation — it is set programmatically after upload
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (session.ImageFile != null && session.ImageFile.Length > 0)
                    {
                        // Upload image to Azurite and store the returned blob URL
                        session.ImageUrl = await _blobService.UploadImageAsync(session.ImageFile);
                    }
                    else
                    {
                        // Fall back to the default placeholder if no file was uploaded
                        session.ImageUrl = "/images/placeholder-session.jpg";
                    }

                    _context.Add(session);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] =
                        $"Medical session '{session.Name}' was added successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", $"Image upload failed: {ex.Message}");
                }
            }

            return View(session);
        }

        // GET: MedicalSessions/Edit/5 — returns the edit form pre-filled with existing data
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.MedicalSessions.FindAsync(id);

            if (session == null) return NotFound();

            return View(session);
        }

        // POST: MedicalSessions/Edit/5 — uploads new image if provided, updates session record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("SessionId,Name,Description,StartDate,EndDate,Category,ImageUrl,ImageFile")] MedicalSession session)
        {
            if (id != session.SessionId) return NotFound();

            if (session.StartDate >= session.EndDate)
                ModelState.AddModelError("EndDate", "End date must be after the start date.");

            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (session.ImageFile != null && session.ImageFile.Length > 0)
                    {
                        var existing = await _context.MedicalSessions
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.SessionId == id);

                        if (existing?.ImageUrl != null &&
                            existing.ImageUrl.StartsWith("http://127.0.0.1"))
                        {
                            await _blobService.DeleteImageAsync(existing.ImageUrl);
                        }

                        session.ImageUrl = await _blobService.UploadImageAsync(session.ImageFile);
                    }
                    else
                    {
                        // Keep the existing image if no new file is uploaded
                        var existing = await _context.MedicalSessions
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.SessionId == id);

                        session.ImageUrl = existing?.ImageUrl ?? "/images/placeholder-session.jpg";
                    }

                    _context.Update(session);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] =
                        $"Medical session '{session.Name}' was updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", $"Image upload failed: {ex.Message}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.SessionId)) return NotFound();
                    else throw;
                }
            }

            return View(session);
        }

        // GET: MedicalSessions/Delete/5 — loads session WITH reservations and facility details
        // for display in the blocked-state table
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.MedicalSessions
                .Include(s => s.Reservations)
                    .ThenInclude(r => r.Facility)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            return View(session);
        }

        // POST: MedicalSessions/Delete/5 — blocks deletion if active reservations exist,
        // redirects back to the Delete view so the blocked state is shown in context
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var session = await _context.MedicalSessions
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            // Block deletion if active reservations are linked to this session.
            if (session.Reservations.Any())
            {
                TempData["ErrorMessage"] =
                    $"Cannot delete '{session.Name}' because it has " +
                    $"{session.Reservations.Count} active reservation(s). " +
                    "Please remove all linked reservations before deleting this session.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            // Delete the blob image from Azurite if it was uploaded there
            if (session.ImageUrl != null &&
                session.ImageUrl.StartsWith("http://127.0.0.1"))
            {
                await _blobService.DeleteImageAsync(session.ImageUrl);
            }

            _context.MedicalSessions.Remove(session);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Medical session '{session.Name}' was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Helper — checks whether a session with the given ID exists
        private bool SessionExists(int id)
        {
            return _context.MedicalSessions.Any(s => s.SessionId == id);
        }
    }
}