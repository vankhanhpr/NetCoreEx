using Microsoft.AspNetCore.Authorization;
using ModelClassLibrary.permission.services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModelClassLibrary.permission.permistionhandle
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IUserPermission m_permission;

        public PermissionHandler(IUserPermission permissionRepository)
        {
            if (permissionRepository == null)
                throw new ArgumentNullException(nameof(permissionRepository));

            m_permission = permissionRepository;
        }


        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return null;
            }

            bool hasPermission = m_permission.checkPermissionForUser(context.User, requirement.Permission);
            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            return Task.FromResult(0); 
        }
    }
}
