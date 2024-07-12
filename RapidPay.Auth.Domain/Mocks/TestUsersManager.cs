﻿using RapidPay.Auth.Adapters.Users;

namespace RapidPay.Auth.Domain.Mocks
{
    public class TestUsersManager : IUsersAdapter
    {
        public bool IsValidUser(string username, string password)
        {
            return username == "testuser" && password == "testpassword";
        }
    }
}
