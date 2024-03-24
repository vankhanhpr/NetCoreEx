
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;


namespace ModelClassLibrary.permission.services
{
    public interface IUserPermission
    {
        Boolean checkPermissionForUser(ClaimsPrincipal user, string permission);
    }
}
