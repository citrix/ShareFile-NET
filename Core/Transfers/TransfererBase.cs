using System.Threading;

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

        protected bool ShouldPause(CancellationToken? cancellationToken)
        {
            if (cancellationToken == null)
                return GetState() == TransfererState.Paused;
            return GetState() == TransfererState.Paused && !cancellationToken.Value.IsCancellationRequested;
        }
    }

    public enum TransfererState
    {
        Active,
        Paused
    }
}
