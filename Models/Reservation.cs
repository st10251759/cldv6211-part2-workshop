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
    // Represents a reservation that links a Facility to a MedicalSession for a specific time slot.
    // A reservation records which facility is reserved, which session it is for,
    // and the scheduled start and end date/time of the reservation.
    public class Reservation
    {
        // Primary key — uniquely identifies each reservation record
        [Key]
        public int ReservationId { get; set; }

        // Foreign key linking this reservation to a Facility.
        // Required to ensure every reservation is associated with a valid facility.
        [Required(ErrorMessage = "Facility is required.")]
        public int FacilityId { get; set; }

        // Navigation property — allows access to the full Facility object.
        // Nullable to support scenarios where facility data is not eagerly loaded.
        public Facility? Facility { get; set; }

        // Foreign key linking this reservation to a MedicalSession.
        // Required to ensure every reservation is associated with a valid session.
        [Required(ErrorMessage = "Medical session is required.")]
        public int SessionId { get; set; }

        // Navigation property — allows access to the full MedicalSession object.
        // Nullable to support scenarios where session data is not eagerly loaded.
        public MedicalSession? MedicalSession { get; set; }

        // The date and time the reservation period begins.
        // Must be before EndDate — validated in the controller.
        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        // The date and time the reservation period ends.
        // Must be after StartDate — validated in the controller.
        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
    }
}