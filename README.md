Flock
=====

DotNet provides a mechanism for locking a range of bytes within a file;
using the [`FileStream.Lock`](https://msdn.microsoft.com/en-us/library/system.io.filestream.lock(v=vs.110).aspx)
method. This will soon be [available in dotnetcore](https://github.com/dotnet/corefx/issues/5964).

However, it is not possible to do this and have the OS notify when the lock is
available; i.e. a blocking lock. Implementing this in fully managed code
requires a spinlock of some kind, that is polling in a loop until we can
obtain the lock.

This is possible on Win32 by using the [`LockFileEx`](https://msdn.microsoft.com/en-us/library/windows/desktop/aa365203(v=vs.85).aspx)
function and IO completion ports; and on Linux by using advisory locking with
[`fcntl`](http://man7.org/linux/man-pages/man2/fcntl.2.html), which simply
blocks the calling thread.

This library exposes these native functions and wraps them in a class `RangeLock`
which models the lifetime of the lock; that is, disposing a `RangeLock` instance
will free its associated lock.

How to use
----------

The following will open a file; obtain a lock on the second byte of the file only;
and write to that second byte. If another process already has a lock that includes
that byte of the file, it will block on the second line until the lock can be
obtained.

```
using (var stream = new FileStream("sample.txt", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
using (var rangeLock = new Win32RangeLock(stream, 1, 1))
{
    stream.Seek(1, SeekOrigin.Begin);
    stream.Write(new byte[] { (byte)'A' }, 0, 1);
}
```

Notes & Quirks
----------------

This implementation uses Linux's "Open File Descriptor" locks, available since Linux 3.1.5.
It would be possible to add an implementation using POSIX advisory locking, however 
this introduces a problem whereby record locks are associated with the process rather than
the file descriptor; and therefore locks may be released unexpectedly.

On the Windows platform; certain guarantees are available about locking on file shares.
See the msdn documentation for `LockFileEx`. The situation is less clear on Linux and
may depend on e.g. the particular NFS implementation. 

Despite some of these pitfalls, it's worth noting that Win32's `LockFileEx` and Unix
advisory locking are both used [in the SQLite project](https://sqlite.org/lockingv3.html#how_to_corrupt)
which gives some indication as to their usefulness.

Todo
----
- There's no timeout to the lock; and no way to cancel the wait! This could be done on Windows
by using [`CancelSynchronousIo`](https://msdn.microsoft.com/en-us/library/windows/desktop/aa363794(v=vs.85).aspx)
and on Linux perhaps with a signal.
- It might be nice to have an `async` interface to this to prevent blocking the thread.
- No OSX implementation - advisory file locking may behave differently on this platform
and I have no way of testing this.