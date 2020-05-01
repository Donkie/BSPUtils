using System;

namespace LibBSP
{
    /// <summary>
    /// Exception thrown when a file doesn't meet the expected format
    /// </summary>
    public class FileFormatException : Exception
    {
        public FileFormatException(string msg) : base(msg)
        {
        }
    }
}