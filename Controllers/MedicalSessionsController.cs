using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediBook.Data;
using MediBook.Models;

namespace MediBook.Controllers
{
    public class MedicalSessionsController : Controller
    {
        private readonly MediBookDbContext _context;

        public MedicalSessionsController(MediBookDbContext context)
        {
            _context = context;
        }

        // GET: MedicalSessions
        public async Task<IActionResult> Index()
        {
            return View(await _context.MedicalSessions.ToListAsync());
        }

        // GET: MedicalSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalSession = await _context.MedicalSessions
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (medicalSession == null)
            {
                return NotFound();
            }

            return View(medicalSession);
        }

        // GET: MedicalSessions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MedicalSessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SessionId,Name,Description,StartDate,EndDate,ImageUrl")] MedicalSession medicalSession)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicalSession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(medicalSession);
        }

        // GET: MedicalSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalSession = await _context.MedicalSessions.FindAsync(id);
            if (medicalSession == null)
            {
                return NotFound();
            }
            return View(medicalSession);
        }

        // POST: MedicalSessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,Name,Description,StartDate,EndDate,ImageUrl")] MedicalSession medicalSession)
        {
            if (id != medicalSession.SessionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(medicalSession);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicalSessionExists(medicalSession.SessionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(medicalSession);
        }

        // GET: MedicalSessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalSession = await _context.MedicalSessions
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (medicalSession == null)
            {
                return NotFound();
            }

            return View(medicalSession);
        }

        // POST: MedicalSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicalSession = await _context.MedicalSessions.FindAsync(id);
            if (medicalSession != null)
            {
                _context.MedicalSessions.Remove(medicalSession);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MedicalSessionExists(int id)
        {
            return _context.MedicalSessions.Any(e => e.SessionId == id);
        }
    }
}
