using System;
using System.Linq;
using System.Web.Mvc;
using LearningManagement.Models;
using LearningManagement.ViewModels;

namespace LearningManagement.Controllers
{
    // Restrict this entire controller to Users only
    [Authorize(Roles = "User")]
    //(Roles = "User")
    public class StudentController : Controller
    {
        private readonly LMSEntities _context = new LMSEntities();

        // Helper method to get the currently logged-in user's ID
        private int GetCurrentUserId()
        {
            var email = User.Identity.Name; // FormsAuth stores Email in Identity.Name
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            return user != null ? user.UserId : 0;
        }

        // GET: Student/Index (Course Catalog & Dashboard)
        public ActionResult Index(string searchTerm, string message, int? categoryId)
        {
            try
            {
                int userId = GetCurrentUserId();

                // 1. Fetch categories for the dropdown filter
                var categories = _context.Categories.ToList();
                ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName", categoryId);

                // Fetch all active courses and determine if the current user is enrolled
                var coursesQuery = _context.Courses.AsQueryable();

                // 2. Apply Search Filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    coursesQuery = coursesQuery.Where(c => c.CourseName.Contains(searchTerm) || c.Category.CategoryName.Contains(searchTerm));
                }

                // 3. Apply Category Filter
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    coursesQuery = coursesQuery.Where(c => c.CategoryId == categoryId.Value);
                }

                var catalogList = coursesQuery.Select(c => new CatalogItemViewModel
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    CategoryName = c.Category.CategoryName,
                    Price = c.Price,
                    // Check if an enrollment record exists for this user and course
                    IsEnrolled = _context.Enrollments.Any(e => e.CourseId == c.CourseId && e.UserId == userId),
                    // Fetch completion percentage if enrolled, otherwise 0
                    CompletionPercentage = _context.Enrollments
                        .Where(e => e.CourseId == c.CourseId && e.UserId == userId)
                        .Select(e => e.CompletionPercentage)
                        .FirstOrDefault() ?? 0
                }).ToList();

                var model = new CatalogViewModel
                {
                    Courses = catalogList,
                    SearchTerm = searchTerm,
                    Message = message
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return Content("An error occurred loading the catalog: " + ex.Message);
            }
        }

        // POST: Student/Enroll

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enroll(int courseId)
        {
            try
            {
                string userEmail = User.Identity.Name;
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

                if (user == null)
                {
                    return RedirectToAction("Index", new { message = "Error: User session invalid or user not found in database." });
                }

                var course = _context.Courses.Find(courseId);
                if (course != null && course.Price > 0)
                {
                    return RedirectToAction("Checkout", new { courseId = courseId });
                }

                int userId = user.UserId;
                bool alreadyEnrolled = _context.Enrollments.Any(e => e.CourseId == courseId && e.UserId == userId);

                if (!alreadyEnrolled)
                {
                    var enrollment = new Enrollment
                    {
                        UserId = userId,
                        CourseId = courseId,
                        EnrollmentDate = DateTime.Now,
                        CompletionPercentage = 0,
                        PaymentStatus = "Free"
                    };

                    _context.Enrollments.Add(enrollment);
                    _context.SaveChanges();

                    return RedirectToAction("Index", new { message = "Successfully enrolled in the course!" });
                }

                return RedirectToAction("Index", new { message = "You are already enrolled in this course." });
            }
            catch (Exception ex)
            {
                string actualError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return RedirectToAction("Index", new { message = "DB Error: " + actualError });
            }
        }


        // GET: Student/Learn
        public ActionResult Learn(int courseId, int? lessonId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var enrollment = _context.Enrollments.FirstOrDefault(e => e.CourseId == courseId && e.UserId == userId);

                if (enrollment == null)
                {
                    return RedirectToAction("Index", new { message = "You must enroll in this course first." });
                }

                var course = _context.Courses.Find(courseId);
                var lessons = _context.Lessons.Where(l => l.CourseId == courseId).OrderBy(l => l.SequenceOrder).ToList();

                if (!lessons.Any())
                {
                    return Content("This course has no lessons yet.");
                }

                var currentLesson = lessonId.HasValue
                    ? lessons.FirstOrDefault(l => l.LessonId == lessonId.Value)
                    : lessons.First();

                if (currentLesson == null) currentLesson = lessons.First();

                decimal completionFraction = (decimal)(enrollment.CompletionPercentage ?? 0) / 100m;
                decimal estimatedSequenceAllowed = (completionFraction * lessons.Count) + 1;

                var model = new LearnViewModel
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CurrentProgress = enrollment.CompletionPercentage ?? 0,
                    CurrentLessonId = currentLesson.LessonId,
                    CurrentLessonTitle = currentLesson.LessonTitle,
                    CurrentContent = currentLesson.Content,
                    CurrentVideoUrl = currentLesson.VideoUrl,
                    CurrentPdfUrl = currentLesson.PdfUrl,
                    IsCurrentCheckpoint = currentLesson.IsCheckpoint ?? false,

                    Lessons = lessons.Select((l, index) => new LessonDisplayModel
                    {
                        LessonId = l.LessonId,
                        LessonTitle = l.LessonTitle,
                        SequenceOrder = l.SequenceOrder,
                        IsCheckpoint = l.IsCheckpoint ?? false,
                        IsLocked = (index + 1) > Math.Ceiling(estimatedSequenceAllowed) && (l.IsCheckpoint ?? false)
                    }).ToList()
                };

                var displayInfo = model.Lessons.First(l => l.LessonId == currentLesson.LessonId);
                if (displayInfo.IsLocked)
                {
                    model.ErrorMessage = "This lesson is locked. You must complete previous checkpoints first.";
                    model.CurrentContent = string.Empty;
                    model.CurrentVideoUrl = string.Empty;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return Content("Error loading lesson: " + ex.Message);
            }
        }

        // GET: Student/Profile
        public ActionResult Profile()
        {
            try
            {
                int userId = GetCurrentUserId();
                var user = _context.Users.Find(userId);

                var model = new StudentProfileViewModel
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email
                };
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { message = "Error loading profile." });
            }
        }

        // POST: Student/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(StudentProfileViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _context.Users.Find(model.UserId);
                    if (user != null)
                    {
                        user.FullName = model.FullName;
                        _context.SaveChanges();
                        model.SuccessMessage = "Profile updated successfully.";
                    }
                }
                // Email is read-only, ensure it repopulates if view is returned
                model.Email = User.Identity.Name;
                return View(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = "Failed to update profile: " + ex.Message;
                model.Email = User.Identity.Name;
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteLesson(int courseId)
        {
            int userId = GetCurrentUserId();
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.CourseId == courseId && e.UserId == userId);

            if (enrollment != null)
            {
                var totalLessons = _context.Lessons.Count(l => l.CourseId == courseId);
                if (totalLessons > 0)
                {
                    decimal increment = 100m / totalLessons;
                    enrollment.CompletionPercentage = (int)Math.Min(100, (enrollment.CompletionPercentage ?? 0) + increment);
                    _context.SaveChanges();
                }
            }

            // Redirect to the next lesson or refresh the current view
            return RedirectToAction("Learn", new { courseId = courseId });
        }


        [HttpGet]
        public ActionResult MyCourses()
        {
            try
            {
                int userId = GetCurrentUserId();

                var myCourses = _context.Enrollments
                    .Where(e => e.UserId == userId)
                    .Select(e => new CatalogItemViewModel
                    {
                        CourseId = e.Cours.CourseId,
                        CourseName = e.Cours.CourseName,
                        Description = e.Cours.Description,
                        CategoryName = e.Cours.Category.CategoryName,
                        Price = e.Cours.Price,
                        IsEnrolled = true,
                        CompletionPercentage = e.CompletionPercentage ?? 0
                    }).ToList();

                return View(myCourses);
            }
            catch (Exception ex)
            {
                return Content("An error occurred loading your courses: " + ex.Message);
            }
        }

        [HttpGet]
        public ActionResult Checkout(int courseId)
        {
            var course = _context.Courses.Find(courseId);
            if (course == null) return HttpNotFound();

            if (course.Price <= 0)
            {
                return RedirectToAction("Enroll", new { courseId = course.CourseId });
            }

            var model = new CheckoutViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Amount = course.Price
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessDummyPayment(CheckoutViewModel model)
        {
            int userId = GetCurrentUserId();

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = model.CourseId,
                EnrollmentDate = DateTime.Now,
                CompletionPercentage = 0,
                PaymentStatus = "Paid"
            };

            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();

            return RedirectToAction("Index", new { message = "Payment successful! You are now enrolled." });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
