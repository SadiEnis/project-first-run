using System;

namespace ProjectFirstRun.Weapons
{
    public readonly struct WeaponFireProfile
    {
        public float PreparationDuration { get; }
        public float CriticalChance { get; }
        public float CriticalMultiplier { get; }
        public WeaponRecoilConfig Recoil { get; }
        public float RecoilDegrees => Recoil.Kick;

        public WeaponFireProfile(float preparationDuration = 0, float criticalChance = 0,
            float criticalMultiplier = 2, float recoilDegrees = 0,
            float recoilRecoveryDelay = .08f, float recoilRecoveryDuration = .3f, float recoilMaximumOffset = 12)
        {
            if (!float.IsFinite(preparationDuration) || preparationDuration < 0)
                throw new ArgumentOutOfRangeException(nameof(preparationDuration));
            if (!float.IsFinite(criticalChance) || criticalChance < 0 || criticalChance > 1)
                throw new ArgumentOutOfRangeException(nameof(criticalChance));
            if (!float.IsFinite(criticalMultiplier) || criticalMultiplier < 1)
                throw new ArgumentOutOfRangeException(nameof(criticalMultiplier));
            if (!float.IsFinite(recoilDegrees) || recoilDegrees < 0)
                throw new ArgumentOutOfRangeException(nameof(recoilDegrees));
            PreparationDuration = preparationDuration;
            CriticalChance = criticalChance;
            CriticalMultiplier = criticalMultiplier;
            Recoil = new WeaponRecoilConfig(recoilDegrees, recoilRecoveryDelay, recoilRecoveryDuration, recoilMaximumOffset);
        }

        public bool RollCritical(Func<double> random)
        {
            if (CriticalChance <= 0) return false;
            if (CriticalChance >= 1) return true;
            if (random == null) throw new ArgumentNullException(nameof(random));
            double sample = random();
            if (double.IsNaN(sample) || sample < 0 || sample >= 1)
                throw new ArgumentOutOfRangeException(nameof(random));
            return sample < CriticalChance;
        }
    }
}
