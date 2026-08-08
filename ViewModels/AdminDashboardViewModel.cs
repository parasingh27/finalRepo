
using System.Collections.Generic;

namespace LearningManagement.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }

        // Chart.js Data Arrays
        public List<string> ChartLabels { get; set; }
        public List<int> ChartData { get; set; }
    }
}
