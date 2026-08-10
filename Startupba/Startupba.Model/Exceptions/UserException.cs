using System;

namespace Startupba.Model
{
    public class UserException : BusinessException
    {
        public UserException(string message) : base(message) { }
        public UserException(string message, Exception inner) : base(message, inner) { }
    }
}
