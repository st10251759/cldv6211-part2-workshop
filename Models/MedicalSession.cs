using System.ComponentModel.DataAnnotations;

/*
==============================Code Attribution==================================
ASP.NET MVC Pattern
Author: Microsoft
Link: https://dotnet.microsoft.com/en-us/apps/aspnet/mvc
Date Accessed: 28 April 2026
==============================Code Attribution==================================
*/

namespace MediBook.Models
{
    // Represents a medical session that can be hosted at a facility.
    // A session has a name, description, schedule and an optional image.
    // It can be linked to multiple reservations across different facilities.
    public class MedicalSession
    {
        // Primary key — uniquely identifies each medical session record
        [Key]
        public int SessionId { get; set; }

        // The name of the medical session.
        // Required and limited to 100 characters.
        [Required(ErrorMessage = "Session name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // A brief description of the medical session.
        // Optional, limited to 300 characters.
        [StringLength(300)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        // The date and time the medical session begins.
        // Must be before EndDate — validated in the controller.
        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        // The date and time the medical session ends.
        // Must be after StartDate — validated in the controller.
        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        // Optional URL pointing to an image representing the medical session.
        // Defaults to the local placeholder image in wwwroot/images.
        // Limited to 500 characters to accommodate long URLs.
        [Display(Name = "Image URL")]
        [StringLength(500)]
        public string? ImageUrl { get; set; } = "/images/placeholder-session.jpg";

        // Navigation property — collection of all reservations linked to this session.
        // Used to check for associated reservations before allowing deletion.
        // Initialised as an empty list to avoid null reference exceptions.
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}