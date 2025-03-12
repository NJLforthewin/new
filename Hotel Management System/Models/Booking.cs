using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Management_System.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public required string GuestName { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [ForeignKey("Room")]
        public int RoomId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public required string Status { get; set; } // Pending, Confirmed, Cancelled

        // Navigation property
        public virtual Room? Room { get; set; }
    }
}
