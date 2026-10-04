using Backend.Fx.Exceptions;
using JetBrains.Annotations;

namespace Backend.Fx.DataSeeding.Feature;

[PublicAPI]
public interface IDataSeedingMutex : IDisposable
{
    bool IsAcquired { get; }

    IDisposable Acquire();
}

public class DataSeedingMutex : IDataSeedingMutex
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private int _disposed;

    public bool IsAcquired => _semaphore.CurrentCount == 0;

    public IDisposable Acquire()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(DataSeedingMutex));
        }

        if (_semaphore.Wait(0))
        {
            return new Release(_semaphore);
        }

        throw new ConflictedException("Data seeding is already running. Aborting.");
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            _semaphore.Dispose();
        }
    }

    private sealed class Release : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private int _released;

        public Release(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                _semaphore.Release();
            }
        }
    }
}