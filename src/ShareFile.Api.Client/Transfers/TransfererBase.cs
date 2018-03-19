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
        
        protected bool ShouldPause(CancellationToken cancellationToken)
        {
            return GetState() == TransfererState.Paused && !cancellationToken.IsCancellationRequested;
        }

		protected bool ShouldPause()
        {
            return GetState() == TransfererState.Paused;
        }
        
		protected async Task TryPauseAsync(CancellationToken cancellationToken)
		{
			while (ShouldPause(cancellationToken))
			{
				await Task.Delay(1000).ConfigureAwait(false);
			}
		}

		// Seems like something we should eliminate entirely with no non-async consumers.
        protected void TryPause()
        {
            while (ShouldPause())
            {
                TryPauseAsync(default(CancellationToken)).Wait();
            }
        }
    }

    public enum TransfererState
    {
        Active,
        Paused
    }
}
