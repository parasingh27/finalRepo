
using System;
using System.Linq;
using System.Web.Mvc;
using LearningManagement.Models;
using LearningManagement.ViewModels;

namespace LMSProject.Controllers
{
    // Restrict User Administration strictly to Admins
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly LMSEntities _context = new LMSEntities();

        // GET: User/Index
        public ActionResult Index(string message)
        {
            try
            {
                var users = _context.Users.Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleName = u.Role.RoleName,
                    IsActive = u.IsActive ?? false
                }).ToList();

                var model = new UserListViewModel
                {
                    Users = users,
                    SuccessMessage = message
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return Content("Error loading users: " + ex.Message);
            }
        }

        // GET: User/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user == null) return HttpNotFound();

                var model = new UserViewModel
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    IsActive = user.IsActive ?? false,
                    // Populate Roles Dropdown
                    RolesList = _context.Roles.Select(r => new SelectListItem
                    {
                        Value = r.RoleId.ToString(),
                        Text = r.RoleName
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { message = "Error loading user details." });
            }
        }

        // POST: User/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _context.Users.Find(model.UserId);
                    if (user != null)
                    {
                        user.FullName = model.FullName;
                        user.RoleId = model.RoleId;
                        user.IsActive = model.IsActive;

                        _context.SaveChanges();
                        return RedirectToAction("Index", new { message = "User updated successfully." });
                    }
                }

                // Reload roles if validation fails
                model.RolesList = _context.Roles.Select(r => new SelectListItem { Value = r.RoleId.ToString(), Text = r.RoleName }).ToList();
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes.");
                model.RolesList = _context.Roles.Select(r => new SelectListItem { Value = r.RoleId.ToString(), Text = r.RoleName }).ToList();
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
