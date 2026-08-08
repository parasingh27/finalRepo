
using System;
using System.Linq;
using System.Web.Security;
using LearningManagement.Models;

namespace LearningManagement.Providers
{
    public class CustomRoleProvider : RoleProvider
    {
        public override string[] GetRolesForUser(string email)
        {
            using (var context = new LMSEntities())
            {
                var userRole = (from u in context.Users
                                join r in context.Roles on u.RoleId equals r.RoleId
                                where u.Email == email
                                select r.RoleName).FirstOrDefault();

                if (userRole != null)
                {
                    return new string[] { userRole };
                }
                return new string[] { };
            }
        }

        // Other abstract methods required by RoleProvider can throw NotImplementedException
        public override void AddUsersToRoles(string[] usernames, string[] roleNames) { throw new NotImplementedException(); }
        public override string ApplicationName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
            public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) { throw new NotImplementedException(); }
        public override string[] FindUsersInRole(string roleName, string usernameToMatch) { throw new NotImplementedException(); }
        public override string[] GetAllRoles() { throw new NotImplementedException(); }
        public override string[] GetUsersInRole(string roleName) { throw new NotImplementedException(); }
        public override bool IsUserInRole(string username, string roleName) { throw new NotImplementedException(); }
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) { throw new NotImplementedException(); }
        public override bool RoleExists(string roleName) { throw new NotImplementedException(); }
    }
}
