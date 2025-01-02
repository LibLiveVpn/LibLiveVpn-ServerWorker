using System.Collections.Concurrent;

namespace LibLiveVpn_ServerWorker.Infrastructure.Services
{
    public class FileAccessorService
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks;

        public FileAccessorService()
        {
            _fileLocks = new();
        }

        public async Task<T> ExecuteActionWithLockedFile<T>(string filePath, Func<Task<T>> operation, CancellationToken cancellationToken)
        {
            var semaphore = _fileLocks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync(cancellationToken);

            try
            {
                return await operation();
            }
            finally
            {
                semaphore.Release();
                TryToCleanLocks(filePath);
            }
        }

        private void TryToCleanLocks(string filePath)
        {
            if (_fileLocks.TryGetValue(filePath, out var semaphore) && semaphore.CurrentCount == 1)
            {
                _fileLocks.TryRemove(filePath, out _);
                semaphore.Dispose();
            }
        }
    }
}
