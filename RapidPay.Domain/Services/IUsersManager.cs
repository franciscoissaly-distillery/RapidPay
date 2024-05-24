namespace RapidPay.Domain.Services
{
    public interface IUsersManager
    {
        bool IsValidUser(string username, string password);
    }

}
