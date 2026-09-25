using System;
using System.Collections.Generic;

namespace ProjectFirstRun.Weapons
{
    public sealed class HitscanVolleyResult
    {
        public IReadOnlyList<HitscanShotResult> Pellets { get; }
        public IReadOnlyList<HitscanShotResult> Targets { get; }
        public bool IsCritical { get; }

        internal HitscanVolleyResult(HitscanShotResult[] pellets, HitscanShotResult[] targets, bool isCritical = false)
        {
            Pellets = Array.AsReadOnly(pellets);
            Targets = Array.AsReadOnly(targets);
            IsCritical = isCritical;
        }
    }
}
