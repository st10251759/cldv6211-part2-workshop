using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // Nullable so that existing records without a category are not affected
        // when this column is added via migration.
        [Display(Name = "Session Category")]
        public SessionCategory? Category { get; set; }

        // The date and time the medical session ends.
        // Must be after StartDate — validated in the controller.
        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        // Stores the Azurite blob URL (or placeholder path) for the session image.
        // Set by the controller after upload — not entered manually by the user.
        // Limited to 500 characters to accommodate long blob URLs.
        [Display(Name = "Image")]
        [StringLength(500)]
        public string? ImageUrl { get; set; } = "/images/placeholder-session.jpg";

        // NOT mapped to the database — used only to receive the uploaded file from the form.
        // The controller reads this, uploads it to Azurite, and stores the returned URL in ImageUrl.
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        // Navigation property — collection of all reservations linked to this session.
        // Used to check for associated reservations before allowing deletion.
        // Initialised as an empty list to avoid null reference exceptions.
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}