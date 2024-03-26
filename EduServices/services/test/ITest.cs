using ModelClass.edu;
using ModelClass.respond;

namespace EduServices.services.test
{
    public interface ITest
    {
        Task<dynamic> test(NLPC_Request rq);
    }
}
