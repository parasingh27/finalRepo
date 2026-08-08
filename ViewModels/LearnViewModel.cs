
using System.Collections.Generic;

namespace LearningManagement.ViewModels
{
    public class LessonDisplayModel
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; }
        public int SequenceOrder { get; set; }
        public bool IsCheckpoint { get; set; }
        public bool IsLocked { get; set; }
    }

    public class LearnViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public decimal CurrentProgress { get; set; }

        // The lesson currently being viewed
        public int CurrentLessonId { get; set; }
        public string CurrentLessonTitle { get; set; }
        public string CurrentContent { get; set; }
        public string CurrentVideoUrl { get; set; }
        public string CurrentPdfUrl { get; set; }
        public bool IsCurrentCheckpoint { get; set; }

        // Sidebar list of lessons
        public List<LessonDisplayModel> Lessons { get; set; }

        public string ErrorMessage { get; set; }
    }
}
