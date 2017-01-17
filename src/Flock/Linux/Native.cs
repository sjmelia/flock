#pragma warning disable SA1300
#pragma warning disable SA1310
#pragma warning disable SA1307
namespace Flock.Linux
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Native functions for Linux fcntl, using open file descriptor locks. See http://man7.org/linux/man-pages/man2/fcntl.2.html
    /// </summary>
    public class Native
    {
        /// <summary>
        /// Constant for setting an open file descriptor lock, blocking.
        /// </summary>
        public const int F_OFD_SETLKW = 38;

        /// <summary>
        /// Constant for setting a POSIX advisory lock, blocking.
        /// </summary>
        public const int F_SETLKW = 0x7;

        /// <summary>
        /// Constant for unlocking fcntl
        /// </summary>
        public const short F_UNLCK = 0x2;

        /// <summary>
        /// Constant for locking fcntl
        /// </summary>
        public const short F_WRLCK = 0x1;

        /// <summary>
        /// Constant for relative seek. (Equivalent of SeekOrigin.Begin in .NET)
        /// </summary>
        public const short SEEK_SET = 0x0;

        /// <summary>
        /// Linux fcntl function.
        /// </summary>
        /// <param name="fd">File descriptor.</param>
        /// <param name="cmd">The fnctl command.</param>
        /// <param name="ptr">Ptr to a struct containing cmd args.</param>
        /// <returns>Success or failure.</returns>
        [DllImport("libc", EntryPoint = "fcntl", SetLastError = true)]
        public static extern int Fcntl(int fd, int cmd, ref flock ptr);

        /// <summary>
        /// Gets the PID of the current process.
        /// </summary>
        /// <returns>The pid of the current process.</returns>
        [DllImport("libc.so.6")]
        public static extern int getpid();

        /// <summary>
        /// Struct for Linux advisory file locking fnctl.
        /// </summary>
        public struct flock
        {
            /// <summary>
            /// The type of the lock.
            /// </summary>
            public short l_type;

            /// <summary>
            /// Seek origin.
            /// </summary>
            public short l_whence;

            /// <summary>
            /// Start of the range to lock.
            /// </summary>
            public long l_start;

            /// <summary>
            /// Length of the range to lock.
            /// </summary>
            public long l_len;

            /// <summary>
            /// PID of process that currently holds the lock. (For F_GETLK or F_OFD_GETLK call)
            /// </summary>
            public int l_pid;
        }
    }
}
#pragma warning restore SA1300
#pragma warning restore SA1310
#pragma warning restore SA1307
