namespace Flock.Tests
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class RangeLockTests
    {
        [Fact]
        public void ShouldNotThrow()
        {
            using (var stream = new FileStream("sample.txt", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var rangeLock = new RangeLock(stream, 1, 1))
            {
                stream.Seek(1, SeekOrigin.Begin);
                stream.Write(new byte[] { (byte)'A' }, 0, 1);
            }
        }

        [Fact]
        public void SecondStreamShouldBlock()
        {
            const int waitTime = 1000;

            var firstThreadInsideLock = new ManualResetEvent(false);
            var allowFirstThreadFinish = new ManualResetEvent(false);
            var secondThreadFinished = new ManualResetEvent(false);
            var t1 = Task.Run(() =>
            {
                using (var stream = new FileStream("sample.txt", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (var rangeLock = new RangeLock(stream, 1, 1))
                {
                    stream.Seek(1, SeekOrigin.Begin);
                    stream.Write(new byte[] { (byte)'A' }, 0, 1);
                    firstThreadInsideLock.Set();
                    allowFirstThreadFinish.WaitOne();
                }
            });

            // Wait until t1 has obtained the lock on the file before proceeding.
            firstThreadInsideLock.WaitOne(waitTime);

            var t2 = Task.Run(() =>
            {
                using (var stream = new FileStream("sample.txt", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (var rangeLock = new RangeLock(stream, 1, 1))
                {
                    stream.Seek(1, SeekOrigin.Begin);
                    stream.Write(new byte[] { (byte)'A' }, 0, 1);
                }
            }).ContinueWith((c2) => secondThreadFinished.Set());

            // confirm that t2 is still running by checking status of manual reset event
            Assert.Equal(false, secondThreadFinished.WaitOne(0));

            // allow t2 to finish by having t1 finish, thus releasing the lock
            allowFirstThreadFinish.Set();

            // now wait on t2 finishing
            secondThreadFinished.WaitOne(waitTime);

            Assert.Equal(true, t2.IsCompleted);
        }
    }
}
