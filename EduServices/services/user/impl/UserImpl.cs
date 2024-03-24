using FuBonServices.data;
using ModelClass.respond;
using ModelClass.user;
using ModelClassLibrary.interfaces;
using WebApi.reponsitory.imlp;

namespace FuBonServices.services.user.impl
{
    public class UserImpl : Reponsitory<User>, IUser
    {
        private IHashPass m_hashPass;
        public UserImpl(DataContext context,IHashPass _hashPass) : base(context)
        {
            m_hashPass = _hashPass;
        }

        //delete
        public DataRespond deleteUser(int usid)
        {
            DataRespond data = new DataRespond();
            try
            {
                delete(usid);
                data.success = true;
                data.message = "Delete a user successfully";
            }
            catch (Exception e)
            {
                data.message = e.Message;   
                data.success = false;
                data.error = e;
            }

            return data;
        }

        //get users
        public DataRespond getUsers()
        {
            DataRespond data = new DataRespond();
            try
            {
                data.success = true;
                data.message = "Get users success";
                data.data = getAll();
            }
            catch (Exception ex)
            {
                data.success = false;
                data.error = ex;
                data.message = ex.Message;
            }
            return data;
        }

        //insert
        public DataRespond insertUser(User rq)
        {
            DataRespond data = new DataRespond();
            try
            {
                insert(rq);
                data.success = true;
                data.message = "Added a user successfully";
                data.data = getAll();
            }
            catch (Exception ex)
            {
                data.success = false;
                data.error = ex;
                data.message = ex.Message;
            }
            return data;
        }
        //update
        public DataRespond updateUser(User rq)
        {
            DataRespond data = new DataRespond();
            try
            {
                var us = getById(rq.usid);
                if (us != null)
                {
                    us.status = rq.status;
                    us.password = m_hashPass.hashPass(rq.password);
                    update(us);
                    data.success = true;
                    data.message = "Added a user successfully";
                    data.data = getAll();
                }
            }
            catch (Exception ex)
            {
                data.success = false;
                data.error = ex;
                data.message = ex.Message;
            }
            return data;
        }
    }
}
