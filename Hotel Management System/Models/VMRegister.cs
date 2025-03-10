using System.ComponentModel.DataAnnotations;

namespace Hotel_Management_System.Models
{
    public class VMRegister
    {
        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
