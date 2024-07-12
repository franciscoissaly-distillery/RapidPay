namespace RapidPay.Auth.Adapters.Users
{
    public interface IUsersAdapter
    // Although still not used by the domain core services,
    // in a more realistic scenario it would most probably evolve and 
    // be consumed by the domain services, so it remains here
    {
        bool IsValidUser(string username, string password);
    }

}
