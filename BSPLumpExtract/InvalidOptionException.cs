using System;

namespace BSPLumpExtract
{
    public class InvalidOptionException : Exception
    {
        public InvalidOptionException(string msg) : base(msg)
        {
        }
    }
}