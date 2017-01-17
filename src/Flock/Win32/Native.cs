namespace Flock.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
    using System.Threading;

    /// <summary>
    /// Native functions for Win32 LockFileEx.
    /// </summary>
    public class Native
    {
        /// <summary>
        /// Flag for an exclusive lock.
        /// </summary>
        public const uint LOCKFILE_EXCLUSIVE_LOCK = 0x00000002;

        /// <summary>
        /// The Win32 LockFileEx function.
        /// </summary>
        /// <param name="handle">File handle.</param>
        /// <param name="flags">Flags for lock.</param>
        /// <param name="mustBeZero">Reserved.</param>
        /// <param name="countLow">Low word of range length.</param>
        /// <param name="countHigh">High word of range length.</param>
        /// <param name="overlapped">Overlapped struct.</param>
        /// <returns>Success or failure.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static unsafe extern bool LockFileEx(SafeFileHandle handle, uint flags, uint mustBeZero, int countLow, int countHigh, NativeOverlapped overlapped);

        /// <summary>
        /// The Win32 UnlockFileEx function.
        /// </summary>
        /// <param name="handle">File handle.</param>
        /// <param name="mustBeZero">Reserved.</param>
        /// <param name="countLow">Low word of range length.</param>
        /// <param name="countHigh">High word of range length.</param>
        /// <param name="overlapped">Overlapped struct.</param>
        /// <returns>Success or failure.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static unsafe extern bool UnlockFileEx(SafeFileHandle handle, uint mustBeZero, int countLow, int countHigh, NativeOverlapped overlapped);
    }
}
