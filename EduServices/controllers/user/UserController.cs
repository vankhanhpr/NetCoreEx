using FuBonServices.services.user;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelClass.respond;
using ModelClass.user;

namespace FuBonServices.controllers.user
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUser m_user;
        public UserController(IUser _user)
        {
            m_user = _user;
        }
        /*
         * Get users
         * khanhnv.hcm
         * 14/02/2024
         */
        [HttpGet("getUsers")]
        public DataRespond getUsers()
        {
            return m_user.getUsers();
        }

        /*
        * insert user
        * khanhnv.hcm
        * 14/02/2024
        */
        [HttpPut("insertUser")]
        public DataRespond insertUser(User rq)
        {
            return m_user.insertUser(rq);
        } 
        /*
        * update a user
        * khanhnv.hcm
        * 14/02/2024
        */
        [HttpPost("updateUser")]
        public DataRespond updateUser(User rq)
        {
            return m_user.updateUser(rq);
        }

        /*
        * update a user
        * khanhnv.hcm
        * 14/02/2024
        */
        [HttpDelete("deleteUser")]
        public DataRespond deleteUser(int usid)
        {
            return m_user.deleteUser(usid);
        }
    }
}
