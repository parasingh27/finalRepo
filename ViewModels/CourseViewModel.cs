
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace LearningManagement.ViewModels
{
    public class CourseViewModel
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Course Name is required.")]
        [StringLength(200, ErrorMessage = "Cannot exceed 200 characters.")]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } // For display in grids
        [Required(ErrorMessage ="Price is required.")]
        [Display(Name ="Course Price(Enter 0 for free)")]
        public decimal Price { get; set; }
        // To populate the Category Dropdown without using ViewBag
        public IEnumerable<SelectListItem> Categories { get; set; }
    }

    // Composite ViewModel for the Index page to handle Pagination, Sorting, and Searching
    public class CourseListViewModel
    {
        public IEnumerable<CourseViewModel> Courses { get; set; }
        public string SearchTerm { get; set; }
        public string SortOrder { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
