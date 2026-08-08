
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace LearningManagement.ViewModels
{
    public class LessonViewModel
    {
        public int LessonId { get; set; }

        public int CourseId { get; set; }
        public string CourseName { get; set; }

        [Required(ErrorMessage = "Lesson Title is required.")]
        [StringLength(200)]
        [Display(Name = "Lesson Title")]
        public string LessonTitle { get; set; }

        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [Display(Name = "Video File")]
        public string VideoUrl { get; set; }

        [Required(ErrorMessage = "Sequence Order is required.")]
        [Display(Name = "Sequence Order")]
        public int SequenceOrder { get; set; }

        // Checkpoint logic: User must complete this lesson to unlock subsequent lessons
        [Display(Name = "Is this a Checkpoint?")]
        public bool IsCheckpoint { get; set; }

        // Handles the file upload from the view
        public HttpPostedFileBase UploadedVideo { get; set; }
        public HttpPostedFileBase UploadedPdf { get; set; }
        public string PdfUrl { get; set; }
    }
}
