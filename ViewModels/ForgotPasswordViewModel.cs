using System.ComponentModel.DataAnnotations;

namespace LearningManagement.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string SecurityQuestion { get; set; }

        public string SecurityAnswer { get; set; }

        public bool IsEmailVerified { get; set; }
    }
}
