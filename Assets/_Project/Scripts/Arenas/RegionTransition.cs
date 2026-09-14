using System;

namespace ProjectFirstRun.Arenas
{
    /// <summary>
    /// Pure transition eligibility and consumption state. Traversal is performed by the caller.
    /// </summary>
    public sealed class RegionTransition
    {
        public string SourceId { get; }
        public string DestinationId { get; }
        public RegionTransitionRequirement Requirement { get; }
        public RegionTransitionTraversal Traversal { get; }
        public RegionTransitionDirection Direction { get; }
        public bool IsConsumed { get; private set; }

        public RegionTransition(
            string sourceId,
            string destinationId,
            RegionTransitionRequirement requirement,
            RegionTransitionTraversal traversal,
            RegionTransitionDirection direction)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
                throw new ArgumentException("A transition requires a source identity.", nameof(sourceId));
            if (string.IsNullOrWhiteSpace(destinationId))
                throw new ArgumentException("A transition requires a destination identity.", nameof(destinationId));
            if (sourceId == destinationId)
                throw new ArgumentException("A transition source and destination must differ.");

            SourceId = sourceId;
            DestinationId = destinationId;
            Requirement = requirement;
            Traversal = traversal;
            Direction = direction;
        }

        public bool CanUse(bool encounterCompleted, bool destinationAvailable)
        {
            if (IsConsumed || !destinationAvailable)
                return false;

            return Requirement == RegionTransitionRequirement.Free ||
                encounterCompleted;
        }

        public bool TryUse(bool encounterCompleted, bool destinationAvailable)
        {
            if (!CanUse(encounterCompleted, destinationAvailable))
                return false;

            if (Direction == RegionTransitionDirection.OneWay)
                IsConsumed = true;

            return true;
        }
    }
}
