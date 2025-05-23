using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class SignUpFormData
{
    [Required]
   // [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]

    public string Email { get; set; } = null!;

    [Required]
   // [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain at least one letter and one number.")]
    public string Password { get; set; } = null!;
}
