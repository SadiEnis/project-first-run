using System;

namespace ProjectFirstRun.Arenas
{
    /// <summary>One connection with explicit direction and a single pending traversal.</summary>
    public sealed class RegionTransition
    {
        public sealed class Attempt
        {
            public string FromId { get; }
            public string ToId { get; }
            internal Attempt(string fromId, string toId)
            {
                FromId = fromId;
                ToId = toId;
            }
        }

        private Attempt _pending;
        public string SourceId { get; }
        public string DestinationId { get; }
        public RegionTransitionRequirement Requirement { get; }
        public RegionTransitionTraversal Traversal { get; }
        public RegionTransitionDirection Direction { get; }
        public bool SingleUse { get; }
        public bool IsConsumed { get; private set; }
        public bool IsInProgress => _pending != null;

        public RegionTransition(string sourceId, string destinationId,
            RegionTransitionRequirement requirement, RegionTransitionTraversal traversal,
            RegionTransitionDirection direction, bool singleUse = false)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
                throw new ArgumentException("A source identity is required.", nameof(sourceId));
            if (string.IsNullOrWhiteSpace(destinationId) || sourceId == destinationId)
                throw new ArgumentException("A distinct destination is required.", nameof(destinationId));
            if (!Enum.IsDefined(typeof(RegionTransitionRequirement), requirement))
                throw new ArgumentOutOfRangeException(nameof(requirement));
            if (!Enum.IsDefined(typeof(RegionTransitionTraversal), traversal))
                throw new ArgumentOutOfRangeException(nameof(traversal));
            if (!Enum.IsDefined(typeof(RegionTransitionDirection), direction))
                throw new ArgumentOutOfRangeException(nameof(direction));
            SourceId = sourceId;
            DestinationId = destinationId;
            Requirement = requirement;
            Traversal = traversal;
            Direction = direction;
            SingleUse = singleUse;
        }

        public bool CanBegin(string fromId, bool encounterCompleted, bool destinationAvailable)
        {
            bool validDirection = fromId == SourceId ||
                (Direction == RegionTransitionDirection.Returnable && fromId == DestinationId);
            return validDirection && !IsConsumed && !IsInProgress && destinationAvailable &&
                (Requirement == RegionTransitionRequirement.Free || encounterCompleted);
        }

        public bool TryBegin(string fromId, bool encounterCompleted,
            bool destinationAvailable, out Attempt attempt)
        {
            attempt = null;
            if (!CanBegin(fromId, encounterCompleted, destinationAvailable))
                return false;
            attempt = new Attempt(fromId, fromId == SourceId ? DestinationId : SourceId);
            _pending = attempt;
            return true;
        }

        public bool Complete(Attempt attempt)
        {
            if (attempt == null || !ReferenceEquals(_pending, attempt)) return false;
            _pending = null;
            IsConsumed = SingleUse;
            return true;
        }

        public bool Cancel(Attempt attempt)
        {
            if (attempt == null || !ReferenceEquals(_pending, attempt)) return false;
            _pending = null;
            return true;
        }
    }
}
