using System;

namespace ProjectFirstRun.Chests
{
    public sealed class ChestState
    {
        public ChestStatus Status
        {
            get;
            private set;
        }

        public bool IsAvailable =>
            Status == ChestStatus.Available;

        public bool IsSelecting =>
            Status == ChestStatus.Selecting;

        public bool IsOpened =>
            Status == ChestStatus.Opened;

        public ChestState()
        {
            Status = ChestStatus.Available;
        }

        public void BeginSelection()
        {
            if (!IsAvailable)
            {
                throw new InvalidOperationException(
                    "A chest selection can only begin while the chest is available.");
            }

            Status = ChestStatus.Selecting;
        }

        public void CancelSelection()
        {
            if (!IsSelecting)
            {
                throw new InvalidOperationException(
                    "A chest selection can only be cancelled while it is active.");
            }

            Status = ChestStatus.Available;
        }

        public void CompleteSelection()
        {
            if (!IsSelecting)
            {
                throw new InvalidOperationException(
                    "A chest can only be opened while its selection is active.");
            }

            Status = ChestStatus.Opened;
        }
    }
}
