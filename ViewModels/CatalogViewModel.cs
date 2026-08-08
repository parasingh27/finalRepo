
using System.Collections.Generic;

namespace LearningManagement.ViewModels
{
    public class CatalogItemViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public bool IsEnrolled { get; set; }
        public decimal CompletionPercentage { get; set; }
        public decimal Price { get; set; }


    }

    public class CatalogViewModel
    {
        public IEnumerable<CatalogItemViewModel> Courses { get; set; }
        public string SearchTerm { get; set; }
        public string Message { get; set; } // To pass success/error messages without ViewBag
    }
}
