using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediBook.Data;
using MediBook.Models;

/*
==============================Code Attribution==================================
ASP.NET MVC Controllers
Author: Microsoft
Link: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
Date Accessed: 28 April 2026
==============================Code Attribution==================================
*/

namespace MediBook.Controllers
{
    // Handles all CRUD operations for MedicalSession records.
    public class MedicalSessionsController : Controller
    {
        private readonly MediBookDbContext _context;

        public MedicalSessionsController(MediBookDbContext context)
        {
            _context = context;
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

        // POST: MedicalSessions/Create — saves the new session to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SessionId,Name,Description,StartDate,EndDate,ImageUrl")] MedicalSession session)
        {
            // Validates that StartDate is before EndDate
            if (session.StartDate >= session.EndDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after the start date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

        // POST: MedicalSessions/Edit/5 — updates the session record in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,Name,Description,StartDate,EndDate,ImageUrl")] MedicalSession session)
        {
            if (id != session.SessionId) return NotFound();

            // Validates that StartDate is before EndDate
            if (session.StartDate >= session.EndDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after the start date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.SessionId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(session);
        }

        // GET: MedicalSessions/Delete/5 — displays delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.MedicalSessions
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            return View(session);
        }

        // POST: MedicalSessions/Delete/5 — removes the session from the database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var session = await _context.MedicalSessions.FindAsync(id);
            if (session != null)
            {
                _context.MedicalSessions.Remove(session);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper — checks whether a session with the given ID exists
        private bool SessionExists(int id)
        {
            return _context.MedicalSessions.Any(s => s.SessionId == id);
        }
    }
}