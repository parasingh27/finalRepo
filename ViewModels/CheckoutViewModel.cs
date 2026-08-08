using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LearningManagement.ViewModels
{
    // Create new CheckoutViewModel
    public class CheckoutViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }
}