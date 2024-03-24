
using System.Security.Claims;

namespace ModelClassLibrary.permission.services.impl
{
    public class UserPermissionImpl:IUserPermission
    {
        public UserPermissionImpl() 
        {
        }
        public bool checkPermissionForUser(ClaimsPrincipal user, string permission)
        {
            //public policy
            if (permission == "public")
            {
                return true;
            } 
            //get username
            var username = user.Claims.FirstOrDefault().Value;
            if(username == "admin")
            {
                return true;
            }
            //get roles for user
            //var pm = from userper in m_context.UserPermissions
            //         join groups in m_context.Groups
            //         on userper.groupid equals groups.groupid
            //         join groupper in m_context.GroupPermissions
            //         on groups.groupid equals groupper.groupid
            //         join perm in m_context.Permissions.Where(n => n.policy == permission)
            //         on groupper.perid equals perm.perid
            //         select userper;

            //-------------------
            //List<User_Permissions> result = new List<User_Permissions>();
            //var conn = GetConnection();
            //if (conn.State == ConnectionState.Closed)
            //{
            //    conn.Open();
            //}
            //string code = "select * from  user_permissions gr_us "+
            //            "join group_permissions gr_per on gr_us.group_id = gr_per.group_id "+
            //            "join permissions per on gr_per.per_id = per.per_id " +
            //            " join users us on us.us_id = gr_us.us_id " +
            //    " where us.username = '" + username + "' and per.policy = '" + permission +"'"
            //    ;
            //result = SqlMapper.Query<User_Permissions>(conn, code, commandType: CommandType.Text).AsList<User_Permissions>();
            //conn.Close();
            //if (result.Count() > 0)
            //{
            //    return true;
            //}
            return false;
        }

    }
}
