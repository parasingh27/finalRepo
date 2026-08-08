
using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LearningManagement.Models;
using LearningManagement.ViewModels;

namespace LearningManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly LMSEntities _context = new LMSEntities();

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            try
            {
                var model = new AdminDashboardViewModel
                {
                    TotalUsers = _context.Users.Count(u => u.Role.RoleName == "User"),
                    TotalCourses = _context.Courses.Count(),
                    TotalEnrollments = _context.Enrollments.Count()
                };

                // Prepare Data for Chart.js (Enrollments per Course)
                var enrollmentStats = _context.Enrollments
                    .GroupBy(e => e.Cours.CourseName)
                    .Select(g => new { CourseName = g.Key, Count = g.Count() })
                    .ToList();

                model.ChartLabels = enrollmentStats.Select(e => e.CourseName).ToList();
                model.ChartData = enrollmentStats.Select(e => e.Count).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                return Content("Error loading dashboard: " + ex.Message);
            }
        }

        // GET: Admin/ExportEnrollmentsCsv
        // Implements the Data Export feature without third-party dependencies
        public ActionResult ExportEnrollmentsCsv()
        {
            try
            {
                var enrollments = _context.Enrollments
                    .Select(e => new
                    {
                        StudentName = e.User.FullName,
                        CourseName = e.Cours.CourseName,
                        Date = e.EnrollmentDate,
                        Progress = e.CompletionPercentage
                    }).ToList();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Student Name,Course Name,Enrollment Date,Progress (%)");

                foreach (var item in enrollments)
                {
                    // Escape commas in strings for CSV compliance
                    string student = $"\"{item.StudentName}\"";
                    string course = $"\"{item.CourseName}\"";
                    string date = item.Date.HasValue ? item.Date.Value.ToString("yyyy-MM-dd") : "N/A";

                    sb.AppendLine($"{student},{course},{date},{item.Progress}");
                }

                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "EnrollmentReport.csv");
            }
            catch (Exception)
            {
                return Content("Failed to generate CSV export.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
