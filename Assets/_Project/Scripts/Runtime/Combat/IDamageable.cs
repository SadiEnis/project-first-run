namespace ProjectFirstRun.Combat
{
    /// <summary>
    /// Contract implemented by objects that can receive damage.
    /// </summary>
    public interface IDamageable
    {
        DamageResult ApplyDamage(in DamageInfo damageInfo);
    }
}