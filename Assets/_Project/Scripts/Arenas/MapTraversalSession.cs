using System;
using System.Collections.Generic;

namespace ProjectFirstRun.Arenas
{
    /// <summary>Shared location and one in-flight traversal for a single-player map.</summary>
    public sealed class MapTraversalSession
    {
        private readonly HashSet<string> _regions = new HashSet<string>(StringComparer.Ordinal);
        private RegionTransition _pendingRoute;
        private RegionTransition.Attempt _pendingAttempt;

        public string CurrentRegionId { get; private set; }
        public bool IsTransitioning => _pendingAttempt != null;

        public MapTraversalSession(string initialRegionId, IEnumerable<string> regionIds)
        {
            if (regionIds == null) throw new ArgumentNullException(nameof(regionIds));
            foreach (string id in regionIds)
                if (string.IsNullOrWhiteSpace(id) || !_regions.Add(id))
                    throw new ArgumentException("Region IDs must be nonempty and unique.", nameof(regionIds));
            if (initialRegionId == null || !_regions.Contains(initialRegionId))
                throw new ArgumentException("The initial region must belong to the map.", nameof(initialRegionId));
            CurrentRegionId = initialRegionId;
        }

        public bool ContainsRoute(RegionTransition route) => route != null &&
            _regions.Contains(route.SourceId) && _regions.Contains(route.DestinationId);

        public bool CanBegin(RegionTransition route, bool encounterCompleted, bool destinationAvailable) =>
            !IsTransitioning && ContainsRoute(route) &&
            route.CanBegin(CurrentRegionId, encounterCompleted, destinationAvailable);

        public bool TryBegin(RegionTransition route, bool encounterCompleted,
            bool destinationAvailable, out RegionTransition.Attempt attempt)
        {
            attempt = null;
            if (!CanBegin(route, encounterCompleted, destinationAvailable) ||
                !route.TryBegin(CurrentRegionId, encounterCompleted, destinationAvailable, out attempt))
                return false;
            _pendingRoute = route;
            _pendingAttempt = attempt;
            return true;
        }

        public bool Complete(RegionTransition route, RegionTransition.Attempt attempt)
        {
            if (!Owns(route, attempt)) return false;
            bool completed = route.Complete(attempt);
            ClearPending();
            if (completed) CurrentRegionId = attempt.ToId;
            return completed;
        }

        public bool Cancel(RegionTransition route, RegionTransition.Attempt attempt)
        {
            if (!Owns(route, attempt)) return false;
            route.Cancel(attempt);
            ClearPending();
            return true;
        }

        private bool Owns(RegionTransition route, RegionTransition.Attempt attempt) =>
            attempt != null && ReferenceEquals(_pendingRoute, route) &&
            ReferenceEquals(_pendingAttempt, attempt);

        private void ClearPending()
        {
            _pendingRoute = null;
            _pendingAttempt = null;
        }
    }
}
