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

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string PhoneNumber { get; set; }

        [Required]
        public int RoomId { get; set; }  // Foreign Key

        [ForeignKey("RoomId")]
        public Room? Room { get; set; }  // Navigation Property

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = "Pending"; // Default status
    }
}
