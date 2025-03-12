using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Management_System.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required]
        public required string RoomNumber { get; set; }

        [Required]
        public required string Category { get; set; } // Standard, Deluxe, Suite

        [Required]
        public required string Status { get; set; } // Vacant, Occupied, Under Maintenance

        [Required]
        [Column(TypeName = "decimal(10,2)")] 
        public decimal PricePerNight { get; set; }


        // Navigation property for bookings
        public virtual List<Booking>? Bookings { get; set; }
    }
}
