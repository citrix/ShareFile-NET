using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class AsyncSemaphore
    {
        private static readonly Task _completed;
        private readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
        private readonly Queue<TaskCompletionSource<bool>> _idleWaiters = new Queue<TaskCompletionSource<bool>>();
        private readonly int _initialCount;
        private int _currentCount;
        private bool _canceled;

        static AsyncSemaphore()
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            _completed = tcs.Task;
        }

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0) throw new ArgumentOutOfRangeException("initialCount");
            _initialCount = initialCount;
            _currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (_waiters)
            {
                if (_currentCount > 0)
                {
                    --_currentCount;
                    
                    return _completed;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    _waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public Task WaitIdleAsync()
        {
            lock (_idleWaiters)
            {
                if (_canceled) return _completed;

                if (_currentCount == _initialCount)
                {
                    return _completed;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    _idleWaiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (_waiters)
            {
                if (_canceled) return;

                if (_waiters.Count > 0)
                    toRelease = _waiters.Dequeue();
                else
                    ++_currentCount;
            }
            if (toRelease != null)
                toRelease.SetResult(true);

            lock (_idleWaiters)
            {
                if (_currentCount == _initialCount)
                    foreach (var w in _idleWaiters) w.SetResult(true);
            }
        }

        public void Cancel()
        {
            _canceled = true;
            lock (_waiters) foreach (var w in _waiters) w.SetException(new TaskCanceledException());
            lock (_idleWaiters) foreach (var w in _idleWaiters) w.SetException(new TaskCanceledException());
        }
    }
}
