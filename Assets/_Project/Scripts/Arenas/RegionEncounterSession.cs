using System;

namespace ProjectFirstRun.Arenas
{
    /// <summary>
    /// Owns one local encounter without coupling its completion to map progression.
    /// The scene owner must dispose this session and separately clean up world objects.
    /// </summary>
    public sealed class RegionEncounterSession : IDisposable
    {
        private IArenaSession _encounter;
        private bool _disposed;

        public string RegionId { get; }
        public RegionEncounterStatus Status { get; private set; }
        public bool IsPlayerInside { get; private set; }

        public event Action Completed;
        public event Action Failed;

        public RegionEncounterSession(string regionId)
        {
            if (string.IsNullOrWhiteSpace(regionId))
                throw new ArgumentException("A region requires a stable identity.", nameof(regionId));

            RegionId = regionId;
        }

        public void Prepare(IArenaSession encounter)
        {
            EnsureNotDisposed();
            if (Status != RegionEncounterStatus.Unprepared)
                throw new InvalidOperationException("The region is already prepared.");
            if (encounter == null)
                throw new ArgumentNullException(nameof(encounter));
            if (!encounter.IsInitialized || encounter.Status != ArenaSessionStatus.Ready)
                throw new InvalidOperationException("The encounter must be initialized and Ready.");

            _encounter = encounter;
            _encounter.Victory += HandleCompleted;
            _encounter.Defeat += HandleFailed;
            Status = RegionEncounterStatus.Prepared;
        }

        public void Enter()
        {
            EnsureNotDisposed();
            if (Status == RegionEncounterStatus.Unprepared)
                throw new InvalidOperationException("Prepare the region before entering.");
            if (Status == RegionEncounterStatus.Failed)
                throw new InvalidOperationException("A failed encounter requires a new run session.");

            IsPlayerInside = true;
            if (Status != RegionEncounterStatus.Prepared)
                return;

            // Set first so synchronous completion and reentrant entry cannot start twice.
            Status = RegionEncounterStatus.Active;
            try
            {
                _encounter.Begin();
            }
            catch
            {
                // A rejected Begin stays retryable; a partially started encounter is not reset.
                if (_encounter.Status == ArenaSessionStatus.Ready)
                {
                    Status = RegionEncounterStatus.Prepared;
                    IsPlayerInside = false;
                }
                throw;
            }
        }

        public void Leave()
        {
            EnsureNotDisposed();
            IsPlayerInside = false;
        }

        private void HandleCompleted()
        {
            if (Status != RegionEncounterStatus.Active)
                return;
            Status = RegionEncounterStatus.Completed;
            Completed?.Invoke();
        }

        private void HandleFailed()
        {
            if (Status != RegionEncounterStatus.Active)
                return;
            Status = RegionEncounterStatus.Failed;
            Failed?.Invoke();
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            IsPlayerInside = false;
            if (_encounter != null)
            {
                _encounter.Victory -= HandleCompleted;
                _encounter.Defeat -= HandleFailed;
            }
            Completed = null;
            Failed = null;
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RegionEncounterSession));
        }
    }
}
