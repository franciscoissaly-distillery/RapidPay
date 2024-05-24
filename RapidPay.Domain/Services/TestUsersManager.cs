namespace RapidPay.Api.Filters
{
    public class TestUsersManager : IUsersManager
    {
        public bool IsValidUser(string username, string password)
        {
            return username == "testuser" && password == "testpassword";
        }
    }

}
