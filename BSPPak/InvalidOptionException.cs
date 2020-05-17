using System;

namespace BSPPak
{
    public class InvalidOptionException : Exception
    {
        public InvalidOptionException(string msg) : base(msg)
        {
        }
    }
}