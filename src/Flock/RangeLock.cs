namespace Flock
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Linux;
    using Win32;

    /// <summary>
    /// Represents a lock on a range of bytes in a file and models its lifetime.
    /// </summary>
    public class RangeLock : IDisposable
    {
        private IDisposable platformSpecificRangeLock;

        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeLock"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor blocks until the lock can be obtained.
        /// </remarks>
        /// <param name="stream">The stream on which to obtain the lock.</param>
        /// <param name="start">The start of the range of bytes to lock.</param>
        /// <param name="length">The length of the range of bytes to lock.</param>
        public RangeLock(FileStream stream, long start, long length)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.platformSpecificRangeLock = new Win32RangeLock(stream, start, length);
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                this.platformSpecificRangeLock = new LinuxRangeLock(stream, start, length);
                return;
            }

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Dispose of the lock, releasing it.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Dispose of the lock, releasing it.
        /// </summary>
        /// <param name="disposing">A value indicating whether we are disposing or finalising.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.platformSpecificRangeLock.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}
