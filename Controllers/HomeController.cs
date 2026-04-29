using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediBook.Data;
using MediBook.Models;

namespace MediBook.Controllers
{
    public class HomeController : Controller
    {
        private readonly MediBookDbContext _context;

        public HomeController(MediBookDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Facilities = await _context.Facilities.ToListAsync();
            ViewBag.Sessions = await _context.MedicalSessions.ToListAsync();
            return View();
        }
    }
}