using System;
using System.Linq;
using System.Web.Mvc;
using LearningManagement.Models;
using LearningManagement.ViewModels;

namespace LearningManagement.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CourseController : Controller
    {
        private readonly LMSEntities _context = new LMSEntities();

        public ActionResult Index(string searchTerm, string sortOrder, int page = 1)
        {
            try
            {
                int pageSize = 5;

                var query = _context.Courses.Select(c => new CourseViewModel
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    CategoryName = c.Category.CategoryName,
                    Price = c.Price
                });

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(c => c.CourseName.Contains(searchTerm) || c.CategoryName.Contains(searchTerm));
                }

                switch (sortOrder)
                {
                    case "name_desc":
                        query = query.OrderByDescending(c => c.CourseName);
                        break;
                    case "category_asc":
                        query = query.OrderBy(c => c.CategoryName);
                        break;
                    default:
                        query = query.OrderBy(c => c.CourseName);
                        break;
                }

                int totalRecords = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                var courses = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var model = new CourseListViewModel
                {
                    Courses = courses,
                    SearchTerm = searchTerm,
                    SortOrder = sortOrder,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return Content("An error occurred while loading courses: " + ex.Message);
            }
        }

        public ActionResult Create()
        {
            var model = new CourseViewModel
            {
                Categories = _context.Categories.Where(c => c.IsActive == true)
                                     .Select(c => new SelectListItem
                                     {
                                         Value = c.CategoryId.ToString(),
                                         Text = c.CategoryName
                                     }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CourseViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var course = new Cours
                    {
                        CourseName = model.CourseName,
                        Description = model.Description,
                        CategoryId = model.CategoryId,
                        Price = model.Price,
                        CreatedAt = DateTime.Now
                    };

                    _context.Courses.Add(course);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }

                model.Categories = _context.Categories.Where(c => c.IsActive == true)
                                         .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save course. Please try again.");
                model.Categories = _context.Categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult DeleteConfirmation(int id)
        {
            var course = _context.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return PartialView("_DeleteConfirmation", course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var course = _context.Courses.Find(id);
                if (course != null)
                {
                    _context.Courses.Remove(course);
                    _context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return Content("Cannot delete this course because it has associated lessons or enrollments.");
            }
        }

        // 1. Add these Edit actions to your CourseController.cs

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var course = _context.Courses.Find(id);
            if (course == null) return HttpNotFound();

            var model = new CourseViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Description = course.Description,
                CategoryId = course.CategoryId,
                Price = course.Price,
                Categories = _context.Categories.Where(c => c.IsActive == true)
                                         .Select(c => new SelectListItem
                                         {
                                             Value = c.CategoryId.ToString(),
                                             Text = c.CategoryName
                                         }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CourseViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var course = _context.Courses.Find(model.CourseId);
                    if (course != null)
                    {
                        course.CourseName = model.CourseName;
                        course.Description = model.Description;
                        course.CategoryId = model.CategoryId;
                        course.Price = model.Price;
                        _context.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                model.Categories = _context.Categories.Where(c => c.IsActive == true)
                                         .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to update course.");
                model.Categories = _context.Categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
                return View(model);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
