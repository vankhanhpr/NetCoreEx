using ModelClass.auth.onebss;
using ModelClass.respond;
using ModelClass.user;
using WebApi.reponsitory;

namespace AuthService.services.auth
{
    public interface IAuth:IReponsitory<User>
    {
        DataRespond login(User user, string lang);
    }
}
