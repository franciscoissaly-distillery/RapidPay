namespace RapidPay.Api.Filters
{
    public interface IUsersManager
    {
        bool IsValidUser(string username, string password);
    }

}
