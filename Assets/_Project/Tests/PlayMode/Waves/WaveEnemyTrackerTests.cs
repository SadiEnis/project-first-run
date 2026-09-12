using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Waves;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Tests.PlayMode.Waves
{
    public sealed class WaveEnemyTrackerTests
    {
        private readonly List<GameObject> _enemyObjects =
            new List<GameObject>();

        private GameObject _targetObject;
        private GameObject _registryObject;

        private EnemyDefinition _enemyDefinition;
        private EnemyRegistry _enemyRegistry;

        [SetUp]
        public void SetUp()
        {
            _targetObject =
                new GameObject("WaveTracker_Target");

            _registryObject =
                new GameObject("WaveTracker_Registry");

            _enemyRegistry =
                _registryObject.AddComponent<EnemyRegistry>();

            _enemyDefinition =
                ScriptableObject.CreateInstance<EnemyDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject enemyObject in _enemyObjects)
            {
                if (enemyObject != null)
                {
                    Object.DestroyImmediate(enemyObject);
                }
            }

            _enemyObjects.Clear();

            if (_targetObject != null)
            {
                Object.DestroyImmediate(_targetObject);
            }

            if (_registryObject != null)
            {
                Object.DestroyImmediate(_registryObject);
            }

            if (_enemyDefinition != null)
            {
                Object.DestroyImmediate(_enemyDefinition);
            }
        }

        [Test]
        public void TrackWave_WithValidEnemies_TracksAllEnemies()
        {
            EnemyController firstEnemy =
                CreateEnemy("Enemy_0");

            EnemyController secondEnemy =
                CreateEnemy("Enemy_1");

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            tracker.TrackWave(
                new[]
                {
                    firstEnemy,
                    secondEnemy
                });

            Assert.That(
                tracker.TrackedEnemyCount,
                Is.EqualTo(2));

            Assert.That(
                tracker.HasTrackedEnemies,
                Is.True);

            Assert.That(
                tracker.Contains(firstEnemy),
                Is.True);

            Assert.That(
                tracker.Contains(secondEnemy),
                Is.True);
        }

        [Test]
        public void TrackWave_WithNullCollection_Throws()
        {
            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            Assert.That(
                () => tracker.TrackWave(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void TrackWave_WithEmptyCollection_Throws()
        {
            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            EnemyController[] enemies =
                System.Array.Empty<EnemyController>();

            Assert.That(
                () => tracker.TrackWave(enemies),
                Throws.ArgumentException);
        }

        [Test]
        public void TrackWave_WithNullEntry_ReportsEntryIndex()
        {
            EnemyController validEnemy =
                CreateEnemy("Enemy_0");

            EnemyController[] enemies =
            {
                validEnemy,
                null
            };

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            System.ArgumentException exception =
                Assert.Throws<System.ArgumentException>(
                    () => tracker.TrackWave(enemies));

            Assert.That(
                exception.Message,
                Does.Contain("index 1"));

            Assert.That(
                tracker.TrackedEnemyCount,
                Is.EqualTo(0));
        }

        [Test]
        public void TrackWave_WithDuplicateEnemy_ReportsEntryIndex()
        {
            EnemyController enemy =
                CreateEnemy("Enemy_0");

            EnemyController[] enemies =
            {
                enemy,
                enemy
            };

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            System.ArgumentException exception =
                Assert.Throws<System.ArgumentException>(
                    () => tracker.TrackWave(enemies));

            Assert.That(
                exception.Message,
                Does.Contain("index 1"));

            Assert.That(
                tracker.TrackedEnemyCount,
                Is.EqualTo(0));
        }

        [Test]
        public void TrackWave_WhilePreviousWaveIsTracked_Throws()
        {
            EnemyController firstEnemy =
                CreateEnemy("Enemy_0");

            EnemyController secondEnemy =
                CreateEnemy("Enemy_1");

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            tracker.TrackWave(
                new[]
                {
                    firstEnemy
                });

            Assert.That(
                () =>
                    tracker.TrackWave(
                        new[]
                        {
                            secondEnemy
                        }),
                Throws.InvalidOperationException);

            Assert.That(
                tracker.TrackedEnemyCount,
                Is.EqualTo(1));

            Assert.That(
                tracker.Contains(firstEnemy),
                Is.True);

            Assert.That(
                tracker.Contains(secondEnemy),
                Is.False);
        }

        [Test]
        public void Contains_WithNullEnemy_ReturnsFalse()
        {
            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            Assert.That(
                tracker.Contains(null),
                Is.False);
        }

        [Test]
        public void EnemyDeath_RemovesEnemyAndPublishesDefeatedEvent()
        {
            EnemyController enemy =
                CreateEnemy("Enemy_0");

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            EnemyController defeatedEnemy = null;
            int defeatedEventCount = 0;

            tracker.EnemyDefeated +=
                defeated =>
                {
                    defeatedEnemy = defeated;
                    defeatedEventCount++;
                };

            tracker.TrackWave(
                new[]
                {
                    enemy
                });

            KillEnemy(enemy);

            Assert.That(
                defeatedEventCount,
                Is.EqualTo(1));

            Assert.That(
                defeatedEnemy,
                Is.SameAs(enemy));

            Assert.That(
                tracker.Contains(enemy),
                Is.False);

            Assert.That(
                tracker.TrackedEnemyCount,
                Is.EqualTo(0));
        }

        [Test]
        public void EnemyDeath_BeforeFinalEnemy_DoesNotPublishAllDefeated()
        {
            EnemyController firstEnemy =
                CreateEnemy("Enemy_0");

            EnemyController secondEnemy =
                CreateEnemy("Enemy_1");

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            int allDefeatedEventCount = 0;

            tracker.AllEnemiesDefeated +=
                () => allDefeatedEventCount++;

            tracker.TrackWave(
                new[]
                {
                    firstEnemy,
                    secondEnemy
                });

            KillEnemy(firstEnemy);

            Assert.That(
                tracker.TrackedEnemyCount,
                Is.EqualTo(1));

            Assert.That(
                tracker.Contains(secondEnemy),
                Is.True);

            Assert.That(
                allDefeatedEventCount,
                Is.EqualTo(0));
        }

        [Test]
        public void FinalEnemyDeath_PublishesAllDefeatedOnce()
        {
            EnemyController firstEnemy =
                CreateEnemy("Enemy_0");

            EnemyController secondEnemy =
                CreateEnemy("Enemy_1");

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            int defeatedEventCount = 0;
            int allDefeatedEventCount = 0;

            tracker.EnemyDefeated +=
                _ => defeatedEventCount++;

            tracker.AllEnemiesDefeated +=
                () => allDefeatedEventCount++;

            tracker.TrackWave(
                new[]
                {
                    firstEnemy,
                    secondEnemy
                });

            KillEnemy(firstEnemy);
            KillEnemy(secondEnemy);

            Assert.That(
                defeatedEventCount,
                Is.EqualTo(2));

            Assert.That(
                allDefeatedEventCount,
                Is.EqualTo(1));

            Assert.That(
                tracker.TrackedEnemyCount,
                Is.EqualTo(0));

            Assert.That(
                tracker.HasTrackedEnemies,
                Is.False);
        }

        [Test]
        public void AdditionalDamageAfterDeath_DoesNotPublishDuplicateEvents()
        {
            EnemyController enemy =
                CreateEnemy("Enemy_0");

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            int defeatedEventCount = 0;
            int allDefeatedEventCount = 0;

            tracker.EnemyDefeated +=
                _ => defeatedEventCount++;

            tracker.AllEnemiesDefeated +=
                () => allDefeatedEventCount++;

            tracker.TrackWave(
                new[]
                {
                    enemy
                });

            KillEnemy(enemy);
            ApplyDamage(enemy, 25f);
            ApplyDamage(enemy, 25f);

            Assert.That(
                defeatedEventCount,
                Is.EqualTo(1));

            Assert.That(
                allDefeatedEventCount,
                Is.EqualTo(1));
        }

        [Test]
        public void DeathOfUntrackedEnemy_DoesNotAffectTracker()
        {
            EnemyController trackedEnemy =
                CreateEnemy("TrackedEnemy");

            EnemyController unrelatedEnemy =
                CreateEnemy("UnrelatedEnemy");

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            int defeatedEventCount = 0;
            int allDefeatedEventCount = 0;

            tracker.EnemyDefeated +=
                _ => defeatedEventCount++;

            tracker.AllEnemiesDefeated +=
                () => allDefeatedEventCount++;

            tracker.TrackWave(
                new[]
                {
                    trackedEnemy
                });

            KillEnemy(unrelatedEnemy);

            Assert.That(
                defeatedEventCount,
                Is.EqualTo(0));

            Assert.That(
                allDefeatedEventCount,
                Is.EqualTo(0));

            Assert.That(
                tracker.TrackedEnemyCount,
                Is.EqualTo(1));

            Assert.That(
                tracker.Contains(trackedEnemy),
                Is.True);
        }

        [Test]
        public void Clear_RemovesSubscriptionsAndTrackedEnemies()
        {
            EnemyController enemy =
                CreateEnemy("Enemy_0");

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            int defeatedEventCount = 0;
            int allDefeatedEventCount = 0;

            tracker.EnemyDefeated +=
                _ => defeatedEventCount++;

            tracker.AllEnemiesDefeated +=
                () => allDefeatedEventCount++;

            tracker.TrackWave(
                new[]
                {
                    enemy
                });

            tracker.Clear();

            Assert.That(
                tracker.TrackedEnemyCount,
                Is.EqualTo(0));

            Assert.That(
                tracker.HasTrackedEnemies,
                Is.False);

            KillEnemy(enemy);

            Assert.That(
                defeatedEventCount,
                Is.EqualTo(0));

            Assert.That(
                allDefeatedEventCount,
                Is.EqualTo(0));
        }

        [Test]
        public void TrackWave_AfterClear_AllowsNewWave()
        {
            EnemyController firstEnemy =
                CreateEnemy("Enemy_0");

            EnemyController secondEnemy =
                CreateEnemy("Enemy_1");

            WaveEnemyTracker tracker =
                new WaveEnemyTracker();

            tracker.TrackWave(
                new[]
                {
                    firstEnemy
                });

            tracker.Clear();

            Assert.DoesNotThrow(
                () =>
                    tracker.TrackWave(
                        new[]
                        {
                            secondEnemy
                        }));

            Assert.That(
                tracker.TrackedEnemyCount,
                Is.EqualTo(1));

            Assert.That(
                tracker.Contains(firstEnemy),
                Is.False);

            Assert.That(
                tracker.Contains(secondEnemy),
                Is.True);
        }

        private EnemyController CreateEnemy(
            string objectName)
        {
            GameObject enemyObject =
                new GameObject(objectName);

            enemyObject.SetActive(false);

            _enemyObjects.Add(enemyObject);

            enemyObject.AddComponent<NavMeshAgent>();
            enemyObject.AddComponent<HealthComponent>();
            enemyObject.AddComponent<EnemyMotor>();

            EnemyController enemyController =
                enemyObject.AddComponent<EnemyController>();

            enemyController.Initialize(
                _enemyDefinition,
                _targetObject.transform,
                _enemyRegistry);

            enemyObject.SetActive(true);

            return enemyController;
        }

        private static void KillEnemy(
            EnemyController enemy)
        {
            ApplyDamage(
                enemy,
                enemy.Health.MaximumHealth);
        }

        private static void ApplyDamage(
            EnemyController enemy,
            float amount)
        {
            DamageInfo damageInfo =
                new DamageInfo(
                    amount,
                    source: null,
                    enemy.transform.position,
                    Vector3.forward);

            enemy.Health.ApplyDamage(
                in damageInfo);
        }
    }
}