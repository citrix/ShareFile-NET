using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers
{
    public abstract class TransfererBase
    {
        public void Pause()
        {
            SetState(TransfererState.Paused);
        }

        public void Resume()
        {
            SetState(TransfererState.Active);
        }

        private TransfererState _state;
        public TransfererState GetState()
        {
            lock (_stateLock)
            {
                return _state;
            }
        }

        private readonly object _stateLock = new object();
        protected void SetState(TransfererState state)
        {
            lock (_stateLock)
            {
                _state = state;
            }
        }

#if Async
        protected bool ShouldPause(CancellationToken? cancellationToken)
        {
            if (cancellationToken == null)
                return GetState() == TransfererState.Paused;
            return GetState() == TransfererState.Paused && !cancellationToken.Value.IsCancellationRequested;
        }
#endif
        protected bool ShouldPause()
        {
            return GetState() == TransfererState.Paused;
        }

#if Async
        protected async Task TryPauseAsync(CancellationToken? cancellationToken)
        {
            while (ShouldPause(cancellationToken))
            {
#if Portable || Net40
                await TaskEx.Delay(1000);
#else
                await Task.Delay(1000);
#endif
            }
        }        
#endif

        protected void TryPause()
        {
            while (ShouldPause())
            {
#if Async
                TryPauseAsync(null).Wait();
#else
                Thread.Sleep(1000);
#endif
            }
        }
    }

    public enum TransfererState
    {
        Active,
        Paused
    }
}
