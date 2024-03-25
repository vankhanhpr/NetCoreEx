using EduServices.services.test;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelClass.respond;

namespace EduServices.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        private ITest m_test;

        public TestController(ITest _test)
        {
            m_test = _test;
        }

        [HttpGet("khanhtest")]
        public async Task<dynamic> test()
        {
            return await m_test.test();
        }


    }
}
