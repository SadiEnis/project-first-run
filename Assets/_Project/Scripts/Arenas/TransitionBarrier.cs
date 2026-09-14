using System;

namespace ProjectFirstRun.Arenas
{
    /// <summary>
    /// Presentation-independent passage barrier state. A view or collider owner
    /// subscribes to Opened/Closed and performs the physical/visual work.
    /// </summary>
    public sealed class TransitionBarrier
    {
        private bool _closeRequested;

        public TransitionBarrierStatus Status { get; private set; } =
            TransitionBarrierStatus.Closed;

        public bool IsPlayerInside { get; private set; }
        public bool IsCloseRequested => _closeRequested;

        public event Action Opened;
        public event Action Closed;

        public bool Open()
        {
            if (Status == TransitionBarrierStatus.Open)
            {
                // Reasserting an open passage cancels a deferred close without
                // publishing a duplicate visual event.
                _closeRequested = false;
                return false;
            }

            Status = TransitionBarrierStatus.Open;
            _closeRequested = false;
            Opened?.Invoke();
            return true;
        }

        public bool NotifyPlayerEntered()
        {
            if (Status != TransitionBarrierStatus.Open || IsPlayerInside)
                return false;

            IsPlayerInside = true;
            return true;
        }

        public bool NotifyPlayerCleared()
        {
            if (!IsPlayerInside)
                return false;

            IsPlayerInside = false;
            if (!_closeRequested)
                return true;

            _closeRequested = false;
            CloseNow();
            return true;
        }

        public bool RequestClose()
        {
            if (Status == TransitionBarrierStatus.Closed)
                return false;

            if (IsPlayerInside)
            {
                _closeRequested = true;
                return false;
            }

            CloseNow();
            return true;
        }

        private void CloseNow()
        {
            Status = TransitionBarrierStatus.Closed;
            _closeRequested = false;
            Closed?.Invoke();
        }
    }
}
