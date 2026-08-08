
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using LearningManagement.Models;
using LearningManagement.ViewModels;
using LearningManagement.Utilities;

namespace LearningManagement.Controllers
{
    public class AuthController : Controller
    {
        private readonly LMSEntities _context = new LMSEntities();

        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                string hashedPassword = SecurityHelper.HashPassword(model.Password);

                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == hashedPassword && u.IsActive == true);

                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.Email, model.RememberMe);

                    var userRole = _context.Roles.FirstOrDefault(r => r.RoleId == user.RoleId)?.RoleName;

                    if (userRole == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else if (userRole == "Manager")
                    {
                        return RedirectToAction("Index", "Course");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Student");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password, or account is inactive.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while logging in. Please try again later.");
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var allowedRoles = _context.Roles.Where(r => r.RoleName == "User" || r.RoleName == "Manager").ToList();
            ViewBag.RoleId = new SelectList(allowedRoles, "RoleId", "RoleName");
            return View(new ViewModels.RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(ViewModels.RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (_context.Users.Any(u => u.Email == model.Email))
                    {
                        ModelState.AddModelError("Email", "An account with this email already exists.");
                        var roles = _context.Roles.Where(r => r.RoleName == "User" || r.RoleName == "Manager").ToList();
                        ViewBag.RoleId = new SelectList(roles, "RoleId", "RoleName", model.RoleId);
                        return View(model);
                    }

                    var newUser = new User
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        PasswordHash = SecurityHelper.HashPassword(model.Password),
                        RoleId = model.RoleId,
                        SecurityQuestion = model.SecurityQuestion,
                        SecurityAnswer = model.SecurityAnswer,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    _context.Users.Add(newUser);
                    _context.SaveChanges();

                    return RedirectToAction("Login", "Auth");
                }

                var allowedRoles = _context.Roles.Where(r => r.RoleName == "User" || r.RoleName == "Manager").ToList();
                ViewBag.RoleId = new SelectList(allowedRoles, "RoleId", "RoleName", model.RoleId);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again later.");
                var allowedRoles = _context.Roles.Where(r => r.RoleName == "User" || r.RoleName == "Manager").ToList();
                ViewBag.RoleId = new SelectList(allowedRoles, "RoleId", "RoleName", model.RoleId);
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Auth");
        }



        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View(new LearningManagement.ViewModels.ForgotPasswordViewModel { IsEmailVerified = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ViewModels.ForgotPasswordViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (!model.IsEmailVerified)
            {
                if (user != null && !string.IsNullOrEmpty(user.SecurityQuestion))
                {
                    ModelState.Clear();
                    model.SecurityQuestion = user.SecurityQuestion;
                    model.IsEmailVerified = true;
                    return View(model);
                }

                ModelState.AddModelError("Email", "Email not found or no security question set.");
                return View(model);
            }

            if (user != null && !string.IsNullOrEmpty(model.SecurityAnswer) && user.SecurityAnswer.Equals(model.SecurityAnswer, StringComparison.OrdinalIgnoreCase))
            {
                string resetToken = Guid.NewGuid().ToString();
                return RedirectToAction("ResetPassword", "Auth", new { token = resetToken, email = model.Email });
            }

            ModelState.AddModelError("SecurityAnswer", "Incorrect security answer.");
            return View(model);
        }

        [HttpGet]
        public ActionResult ResetPassword(string token, string email)
        {
            var model = new ViewModels.ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ViewModels.ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
                user.PasswordHash = SecurityHelper.HashPassword(model.NewPassword);
                _context.SaveChanges();
                return RedirectToAction("Login", "Auth");
            }

            ModelState.AddModelError("", "Invalid request.");
            return View(model);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
