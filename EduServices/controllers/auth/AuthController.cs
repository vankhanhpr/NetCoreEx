using AuthService.services.auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelClass.respond;
using ModelClass.user;

namespace FuBonServices.controllers.auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IAuth m_auth;
        public AuthController(IAuth _auth)
        {
            m_auth = _auth;
        }
        /*
         * Create 14/02/2024
         * Login all web and mobile
         * Rq: username and password
         */
        [HttpPost("login")]
        public DataRespond login(User us)
        {
            DataRespond data = new DataRespond();

            
            return m_auth.login(us,"vn");
        }
    }
}
