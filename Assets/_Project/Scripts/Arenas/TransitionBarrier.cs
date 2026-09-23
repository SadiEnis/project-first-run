using System;
using System.Collections.Generic;

namespace ProjectFirstRun.Arenas
{
    /// <summary>
    /// Presentation-independent passage barrier state. A view or collider owner
    /// subscribes to Opened/Closed and performs the physical/visual work.
    /// </summary>
    public sealed class TransitionBarrier
    {
        private bool _closeRequested;
        private readonly HashSet<int> _occupants = new HashSet<int>();

        public TransitionBarrierStatus Status { get; private set; } =
            TransitionBarrierStatus.Closed;

        public bool IsPlayerInside => _occupants.Count != 0;
        public int OccupantCount => _occupants.Count;
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

        public bool NotifyPlayerEntered(int colliderId)
        {
            return _occupants.Add(colliderId);
        }

        public bool NotifyPlayerCleared(int colliderId)
        {
            if (!_occupants.Remove(colliderId))
                return false;

            if (IsPlayerInside || !_closeRequested)
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
