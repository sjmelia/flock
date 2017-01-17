namespace Flock.Linux
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a range lock on the dotnet Linux platform using fcntl.
    /// </summary>
    public class LinuxRangeLock : IDisposable
    {
        private int fd = -1;

        private long start;

        private long length;

        private bool disposedValue = false;

        /// <summary>
        /// Intializes a new instance of the LinuxRangeLock class.
        /// </summary>
        /// <param name="stream">The stream on which to obtain the lock.</param>
        /// <param name="start">The start of the range of bytes to lock.</param>
        /// <param name="length">The length of the range of bytes to lock.</param>
        public LinuxRangeLock(FileStream stream, long start, long length)
        {
            this.start = start;
            this.length = length;
            var fl = new Native.flock()
            {
                l_type = Native.F_WRLCK,
                l_whence = Native.SEEK_SET,
                l_start = this.start,
                l_len = this.length,
                l_pid = Native.getpid()
            };
            this.fd = stream.SafeFileHandle.DangerousGetHandle().ToInt32();
            var result = Native.Fcntl(fd, Native.F_OFD_SETLKW, ref fl);
            if (result != 0)
            {
                var errno = Marshal.GetLastWin32Error();
                throw new RangeLockException(errno);
            }
        }

        /// <summary>
        /// Dispose of the range lock.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the range lock.
        /// </summary>
        /// <param name="disposing">A value indicating whether we are finalising or disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                this.ReleaseLock();

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        private void ReleaseLock()
        {
            if (fd == -1)
            {
                return;
            }

            var fl = new Native.flock()
            {
                l_type = Native.F_UNLCK,
                l_whence = Native.SEEK_SET,
                l_start = this.start,
                l_len = this.length,
                l_pid = Native.getpid()
            };

            var result = Native.Fcntl(fd, Native.F_OFD_SETLKW, ref fl);
            if (result != 0)
            {
                var errno = Marshal.GetLastWin32Error();
                throw new RangeLockException(errno);
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~LinuxRangeLock()
        {
            this.Dispose(false);
        }
    }
}
