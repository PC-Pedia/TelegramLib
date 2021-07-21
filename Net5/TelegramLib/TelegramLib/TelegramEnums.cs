using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramLib
{
    public class TelegramEnums
    {
        public enum ClientStatus
        {
            NotStarted = 0,
            Started = 1
        }

        public enum UserLoginStatus
        {
            Unauthorized = 0,
            LoginNeeded = 1,
            WaitForActivationCode = 2,
            WaitForPassword = 3,
            Logined = 4
        }
    }
}
