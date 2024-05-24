using RapidPay.Domain.Adapters;

namespace RapidPay.Auth.Mocks
{
    public class TestUsersManager : IUsersAdapter
    {
        public bool IsValidUser(string username, string password)
        {
            return username == "testuser" && password == "testpassword";
        }
    }
}
