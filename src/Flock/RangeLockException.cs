namespace Flock
{
    using System;

    /// <summary>
    /// Represents an exception while trying to obtain a range lock.
    /// </summary>
    public class RangeLockException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the RangeLockException class.
        /// </summary>
        /// <param name="errorCode">The system error code of the exception.</param>
        public RangeLockException(int errorCode)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets or sets the system error code.
        /// </summary>
        public int ErrorCode { get; set; }
    }
}
