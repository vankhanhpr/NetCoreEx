using ModelClass.respond;
using ModelClass.user;
using WebApi.reponsitory;

namespace FuBonServices.services.user
{
    public interface IUser : IReponsitory<User>
    {
        DataRespond getUsers();
        DataRespond insertUser(User rq);
        DataRespond updateUser(User rq);
        DataRespond deleteUser(int usid);
    }
}
