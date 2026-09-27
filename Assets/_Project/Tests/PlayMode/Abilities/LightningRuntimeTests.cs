#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Lightning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
namespace ProjectFirstRun.Tests.PlayMode.Abilities
{
    public sealed class LightningRuntimeTests
    {
        private readonly List<Object> _objects = new List<Object>();
        private NavMeshDataInstance _mesh;
        private EnemyRegistry _registry;
        private HealthComponent _player;
        private AbilityRuntimeEntry _entry;
        private GameObject Make(string name)
        {
            var go = new GameObject(name); _objects.Add(go); return go;
        }
        [SetUp] public void SetUp()
        {
            Time.timeScale = 1;
            var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0),
                new List<NavMeshBuildSource> { new NavMeshBuildSource {
                    shape = NavMeshBuildSourceShape.Box, size = new Vector3(50, .2f, 50),
                    transform = Matrix4x4.TRS(new Vector3(0, -.1f, 0), Quaternion.identity, Vector3.one), area = 0
                } }, new Bounds(Vector3.zero, new Vector3(50, 5, 50)), Vector3.zero, Quaternion.identity);
            _objects.Add(data); _mesh = NavMesh.AddNavMeshData(data);
            _registry = Make("Lightning registry").AddComponent<EnemyRegistry>();
            _player = Make("Lightning source").AddComponent<HealthComponent>(); _player.Initialize(100);
            var asset = AssetDatabase.LoadAssetAtPath<LightningDefinition>("Assets/_Project/Data/Abilities/AD_LightningStaff.asset");
            _entry = new LightningRuntimeFactory(_registry, _player.gameObject, new PlayerStatCollection()).Create(asset);
        }
        private EnemyController Enemy(float x)
        {
            var go = Make("Lightning target"); go.transform.position = new Vector3(x, 0, 0);
            go.AddComponent<NavMeshAgent>();
            var enemy = go.AddComponent<EnemyController>();
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>(); _objects.Add(definition);
            enemy.Initialize(definition, _player.transform, _registry);
            enemy.Health.Initialize(1000); go.GetComponent<EnemyMotor>().Stop();
            return enemy;
        }
        private void Level(int level)
        {
            var advance = typeof(AbilityRuntimeEntry).GetMethod("AdvanceLevel", BindingFlags.Instance | BindingFlags.NonPublic);
            while (_entry.Level < level) advance.Invoke(_entry, null);
        }
        private AbilityAutoCastResult Cast()
        {
            Physics.SyncTransforms();
            return _entry.TryAutoCast(Vector3.up);
        }
        [TearDown] public void TearDown()
        {
            Time.timeScale = 1;
            foreach (var visual in Object.FindObjectsByType<LightningVisual>(FindObjectsSortMode.None))
                Object.DestroyImmediate(visual.gameObject);
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
            if (_mesh.valid) _mesh.Remove();
        }
        [Test] public void AreaHitDeduplicatesColliders()
        {
            var a = Enemy(3); var b = Enemy(4);
            a.gameObject.AddComponent<BoxCollider>(); a.gameObject.AddComponent<SphereCollider>();
            Assert.That(Cast(), Is.EqualTo(AbilityAutoCastResult.Performed));
            Assert.That(a.Health.CurrentHealth, Is.EqualTo(980));
            Assert.That(b.Health.CurrentHealth, Is.EqualTo(980));
        }
        [Test] public void WallBlocksTargetAndDoesNotConsumeCooldown()
        {
            Enemy(3);
            var wall = Make("Wall"); wall.transform.position = new Vector3(1.5f, 1, 0);
            wall.AddComponent<BoxCollider>().size = new Vector3(.2f, 4, 5);
            Assert.That(Cast(), Is.EqualTo(AbilityAutoCastResult.NoTarget));
            Assert.That(_entry.State.CooldownRemaining, Is.Zero);
        }
        [Test] public void MultipleStrikesReuseOnlyTargetAndApplyStun()
        {
            var enemy = Enemy(3); Level(7);
            Assert.That(Cast(), Is.EqualTo(AbilityAutoCastResult.Performed));
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(880));
            Assert.That(enemy.GetComponent<EnemyMotor>().IsStunned, Is.True);
            Assert.That(_entry.State.CooldownRemaining, Is.EqualTo(3));
        }
        [Test] public void ChainHitsOneAdditionalTargetWithoutRecursion()
        {
            var primary = Enemy(13); var chain = Enemy(16); var beyond = Enemy(19);
            Level(8); Cast();
            Assert.That(primary.Health.CurrentHealth, Is.EqualTo(880));
            Assert.That(chain.Health.CurrentHealth, Is.EqualTo(880));
            Assert.That(beyond.Health.CurrentHealth, Is.EqualTo(1000));
        }
        [Test] public void ChainDoesNotHitAreaVictimTwice()
        {
            var a = Enemy(3); var b = Enemy(4); Level(8); Cast();
            Assert.That(a.Health.CurrentHealth, Is.EqualTo(880));
            Assert.That(b.Health.CurrentHealth, Is.EqualTo(880));
        }
        [Test] public void RebindingUsesOnlyNewMapRegistry()
        {
            var oldEnemy = Enemy(3);
            var next = Make("Next registry").AddComponent<EnemyRegistry>();
            _entry.BindEnemyRegistry(next);
            Assert.That(Cast(), Is.EqualTo(AbilityAutoCastResult.NoTarget));
            Assert.That(oldEnemy.Health.CurrentHealth, Is.EqualTo(1000));
        }
        [Test] public void ChainCannotCrossWall()
        {
            Enemy(13); var chain = Enemy(16);
            var wall = Make("Chain wall"); wall.transform.position = new Vector3(14.5f, 1, 0);
            wall.AddComponent<BoxCollider>().size = new Vector3(.2f, 4, 5);
            Level(8); Cast();
            Assert.That(chain.Health.CurrentHealth, Is.EqualTo(1000));
        }
        [Test] public void LevelAdvancePreservesRemainingCooldown()
        {
            Enemy(3); Cast();
            _entry.Tick(1);
            float remaining = _entry.State.CooldownRemaining;
            Level(8);
            Assert.That(_entry.State.CooldownRemaining, Is.EqualTo(remaining));
            _entry.Tick(10); Cast();
            Assert.That(_entry.State.CooldownRemaining, Is.EqualTo(3));
        }
        [Test] public void LethalStrikeUnregistersSafelyAndClearsStun()
        {
            var enemy = Enemy(3); var motor = enemy.GetComponent<EnemyMotor>();
            motor.ApplyStun(1);
            enemy.Health.Initialize(10);
            Assert.That(Cast(), Is.EqualTo(AbilityAutoCastResult.Performed));
            Assert.That(_registry.ActiveCount, Is.Zero);
            Assert.That(motor.IsStunned, Is.False);
        }
        [Test] public void PauseAndSourceDeathPreventDamage()
        {
            var enemy = Enemy(3);
            Time.timeScale = 0;
            Assert.That(Cast(), Is.EqualTo(AbilityAutoCastResult.ExecutionFailed));
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(1000));
            Time.timeScale = 1;
            var damage = new DamageInfo(1000, null, Vector3.zero, Vector3.up);
            _player.ApplyDamage(in damage);
            Assert.That(Cast(), Is.EqualTo(AbilityAutoCastResult.NoTarget));
        }
        [UnityTest] public IEnumerator StunRefreshPauseExpiryAndReusePreserveSlow()
        {
            var enemy = Enemy(3); var motor = enemy.GetComponent<EnemyMotor>();
            object acid = new object(); motor.SetMovementModifier(acid, .5f);
            motor.ApplyStun(.3f);
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(.35f);
            Assert.That(motor.IsStunned, Is.True);
            Time.timeScale = 1;
            motor.ApplyStun(.1f); // A shorter application must not shorten existing stun.
            yield return new WaitForSeconds(.15f);
            Assert.That(motor.IsStunned, Is.True);
            yield return new WaitForSeconds(.25f);
            Assert.That(motor.IsStunned, Is.False);
            Assert.That(motor.MovementMultiplier, Is.EqualTo(.5f));
            motor.ApplyStun(1); enemy.gameObject.SetActive(false);
            Assert.That(motor.IsStunned, Is.False);
        }
    }
}
#endif
