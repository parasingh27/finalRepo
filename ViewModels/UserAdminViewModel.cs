

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace LearningManagement.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public string Email { get; set; } // Read-only for Admin

        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        public string RoleName { get; set; }

        [Display(Name = "Active Account")]
        public bool IsActive { get; set; }

        // Dropdown for Role Selection
        public IEnumerable<SelectListItem> RolesList { get; set; }
    }

    public class UserListViewModel
    {
        public IEnumerable<UserViewModel> Users { get; set; }
        public string SuccessMessage { get; set; }
    }
}
