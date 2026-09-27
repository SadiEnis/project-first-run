using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Shuriken
{
    // Reuse the tick clock, not TimedBurn's source-death cancellation policy.
    public sealed class ShurikenBleed : MonoBehaviour
    {
        private GameObject _source;
        private EnemyController _enemy;
        private int _spawnVersion;
        private bool _ended;
        public TimedBurnState State { get; } = new TimedBurnState();
        public static void Apply(GameObject source, EnemyController enemy, float damage)
        {
            if (enemy == null || enemy.IsDead || !enemy.isActiveAndEnabled) return;
            ShurikenBleed effect = null;
            foreach (var candidate in enemy.GetComponents<ShurikenBleed>())
                if (!candidate._ended && candidate._source == source && candidate._spawnVersion == enemy.SpawnVersion)
                { effect = candidate; break; }
            if (effect == null)
            {
                effect = enemy.gameObject.AddComponent<ShurikenBleed>();
                effect._source = source; effect._enemy = enemy; effect._spawnVersion = enemy.SpawnVersion;
            }
            effect.State.Refresh(damage, 2);
        }
        private void Update() => Advance(Time.deltaTime);
        public void Advance(float delta)
        {
            if (_ended) return;
            if (_enemy == null || !_enemy.isActiveAndEnabled || _enemy.IsDead || _enemy.Health.IsDead ||
                _enemy.SpawnVersion != _spawnVersion) { End(); return; }
            int ticks = State.Advance(delta);
            for (int i = 0; i < ticks && !_ended && _enemy != null && _enemy.isActiveAndEnabled &&
                !_enemy.IsDead && _enemy.SpawnVersion == _spawnVersion; i++)
            {
                var info = new DamageInfo(State.Damage, _source, transform.position, Vector3.up);
                _enemy.Health.ApplyDamage(in info);
            }
            if (State.Remaining <= 0 || _enemy == null || _enemy.IsDead || _enemy.SpawnVersion != _spawnVersion) End();
        }
        private void OnDisable() => End();
        private void End() { if (_ended) return; _ended = true; Destroy(this); }
    }
}
