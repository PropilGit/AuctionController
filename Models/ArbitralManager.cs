using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionController.Models
{
    class ArbitralManager
    {
        public ArbitralManager(string name, string shortName, string login, string password)
        {
            Name = name;
            ShortName = shortName;
            Login = login;
            Password = password;
        }

        public string Name{ get; private set; }
        public string ShortName { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
    }
}
