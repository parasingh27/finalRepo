using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using LearningManagement.Models;
using LearningManagement.ViewModels;

namespace LearningManagement.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class LessonController : Controller
    {
        private readonly LMSEntities _context = new LMSEntities();

        public ActionResult Index(int? courseId)
        {
            if (courseId == null)
            {
                return RedirectToAction("Index", "Course");
            }

            var course = _context.Courses.Find(courseId);
            if (course == null) return HttpNotFound();

            ViewBag.CourseName = course.CourseName;
            ViewBag.CourseId = course.CourseId;

            var lessons = _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.SequenceOrder)
                .ToList();

            return View(lessons);
        }

        public ActionResult Create(int courseId)
        {
            var course = _context.Courses.Find(courseId);
            if (course == null) return HttpNotFound();

            var model = new LessonViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                SequenceOrder = _context.Lessons.Count(l => l.CourseId == courseId) + 1
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LessonViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string savedVideoPath = null;
                    string savedPdfPath = null;

                    if (model.UploadedVideo != null && model.UploadedVideo.ContentLength > 0)
                    {
                        var allowedExtensions = new[] { ".mp4", ".avi", ".mkv" };
                        var extension = Path.GetExtension(model.UploadedVideo.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("UploadedVideo", "Please upload a valid video file (.mp4, .avi, .mkv).");
                            return View(model);
                        }

                        string fileName = Guid.NewGuid().ToString() + extension;
                        string directoryPath = Server.MapPath("~/Content/Uploads/Videos/");

                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);

                        string filePath = Path.Combine(directoryPath, fileName);
                        model.UploadedVideo.SaveAs(filePath);
                        savedVideoPath = "/Content/Uploads/Videos/" + fileName;
                    }

                    if (model.UploadedPdf != null && model.UploadedPdf.ContentLength > 0)
                    {
                        var allowedExtensions = new[] { ".pdf" };
                        var extension = Path.GetExtension(model.UploadedPdf.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("UploadedPdf", "Please upload a valid PDF file.");
                            return View(model);
                        }

                        string fileName = Guid.NewGuid().ToString() + extension;
                        string directoryPath = Server.MapPath("~/Content/Uploads/Pdfs/");

                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);

                        string filePath = Path.Combine(directoryPath, fileName);
                        model.UploadedPdf.SaveAs(filePath);
                        savedPdfPath = "/Content/Uploads/Pdfs/" + fileName;
                    }

                    var lesson = new Lesson
                    {
                        CourseId = model.CourseId,
                        LessonTitle = model.LessonTitle,
                        Content = model.Content,
                        VideoUrl = savedVideoPath,
                        PdfUrl = savedPdfPath,
                        SequenceOrder = model.SequenceOrder,
                        IsCheckpoint = model.IsCheckpoint
                    };

                    _context.Lessons.Add(lesson);
                    _context.SaveChanges();

                    return RedirectToAction("Index", "Lesson", new { courseId = model.CourseId });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving lesson: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var lesson = _context.Lessons.Find(id);
            if (lesson == null) return HttpNotFound();

            int courseId = lesson.CourseId;

            if (!string.IsNullOrEmpty(lesson.VideoUrl))
            {
                string fullVideoPath = Server.MapPath("~" + lesson.VideoUrl);
                if (System.IO.File.Exists(fullVideoPath))
                {
                    System.IO.File.Delete(fullVideoPath);
                }
            }

            if (!string.IsNullOrEmpty(lesson.PdfUrl))
            {
                string fullPdfPath = Server.MapPath("~" + lesson.PdfUrl);
                if (System.IO.File.Exists(fullPdfPath))
                {
                    System.IO.File.Delete(fullPdfPath);
                }
            }

            _context.Lessons.Remove(lesson);
            _context.SaveChanges();

            return RedirectToAction("Index", new { courseId = courseId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
