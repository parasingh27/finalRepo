
using System.ComponentModel.DataAnnotations;

namespace LearningManagement.ViewModels
{
    public class StudentProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; } // Read-only in profile

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
    }
}
