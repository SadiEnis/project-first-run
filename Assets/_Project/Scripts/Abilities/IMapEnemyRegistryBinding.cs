using ProjectFirstRun.Enemies;

namespace ProjectFirstRun.Abilities
{
    /// <summary>Rebind scene-owned targeting without rebuilding ability runtime state.</summary>
    public interface IMapEnemyRegistryBinding
    {
        void BindEnemyRegistry(EnemyRegistry registry);
    }
}
