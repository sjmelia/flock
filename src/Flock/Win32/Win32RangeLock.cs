namespace Flock.Win32
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    /// <summary>
    /// Represents a range lock on the Win32 platform, using LockFileEx.
    /// </summary>
    public class Win32RangeLock : IDisposable
    {
        private FileStream stream;

        private long start;

        private long length;

        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Win32RangeLock"/> class.
        /// </summary>
        /// <remarks>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa365683(v=vs.85).aspx
        /// @todo: cancellation https://msdn.microsoft.com/en-us/library/windows/desktop/aa363794(v=vs.85).aspx
        /// </remarks>
        /// <param name="stream">The stream on which to obtain the lock.</param>
        /// <param name="start">The start of the range of bytes to lock.</param>
        /// <param name="length">The length of the range of bytes to lock.</param>
        public Win32RangeLock(FileStream stream, long start, long length)
        {
            this.stream = stream;
            this.start = start;
            this.length = length;

            this.Lock(this.stream, this.start, this.length);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Win32RangeLock"/> class.
        /// </summary>
        ~Win32RangeLock()
        {
           this.Dispose(false);
        }

        /// <summary>
        /// Disposes the range lock, releasing it.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the range lock, releasing it.
        /// </summary>
        /// <param name="disposing">A value indicating whether we are finalising or disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                this.Unlock(this.stream, this.start, this.length);

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Lock a file using the Win32 LockFileEx function.
        /// </summary>
        /// <param name="stream">The stream on which to obtain the lock.</param>
        /// <param name="start">The start of the range of bytes to lock.</param>
        /// <param name="length">The length of the range of bytes to lock.</param>
        private void Lock(FileStream stream, long start, long length)
        {
            var resetEvent = new ManualResetEvent(false);
            var handle = stream.SafeFileHandle;

            NativeOverlapped overlapped = new NativeOverlapped();
            overlapped.OffsetHigh = (int)(start >> 32);
            overlapped.OffsetLow = (int)start;
            overlapped.EventHandle = resetEvent.GetSafeWaitHandle().DangerousGetHandle();

            uint flags = Native.LOCKFILE_EXCLUSIVE_LOCK;
            if (!Native.LockFileEx(handle, flags, 0, (int)length, (int)(length >> 32), overlapped))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new RangeLockException(errorCode);
            }

            resetEvent.WaitOne();
            resetEvent.Reset();
        }

        /// <summary>
        /// Unlock a file using the Win32 UnlockFileEx function.
        /// </summary>
        /// <param name="stream">The stream on which to release the lock.</param>
        /// <param name="start">The start of the range of bytes to unlock.</param>
        /// <param name="length">The length of the range of bytes to unlock.</param>
        private void Unlock(FileStream stream, long start, long length)
        {
            var handle = stream.SafeFileHandle;

            NativeOverlapped overlapped = new NativeOverlapped();
            overlapped.OffsetHigh = (int)(start >> 32);
            overlapped.OffsetLow = (int)start;

            if (!Native.UnlockFileEx(handle, 0, (int)length, (int)(length >> 32), overlapped))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new RangeLockException(errorCode);
            }
        }
    }
}
