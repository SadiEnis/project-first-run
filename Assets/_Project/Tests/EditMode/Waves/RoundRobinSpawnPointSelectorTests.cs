using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Waves;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Waves
{
    public sealed class RoundRobinSpawnPointSelectorTests
    {
        private readonly List<GameObject> _createdObjects =
            new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject createdObject in _createdObjects)
            {
                if (createdObject != null)
                {
                    Object.DestroyImmediate(createdObject);
                }
            }

            _createdObjects.Clear();
        }

        [Test]
        public void Constructor_WithValidSpawnPoints_StoresInitialState()
        {
            Transform firstPoint =
                CreateSpawnPoint("Point_0");

            Transform secondPoint =
                CreateSpawnPoint("Point_1");

            RoundRobinSpawnPointSelector selector =
                new RoundRobinSpawnPointSelector(
                    new[]
                    {
                        firstPoint,
                        secondPoint
                    });

            Assert.That(
                selector.Count,
                Is.EqualTo(2));

            Assert.That(
                selector.NextIndex,
                Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithNullCollection_Throws()
        {
            Assert.That(
                () => new RoundRobinSpawnPointSelector(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithEmptyCollection_Throws()
        {
            Transform[] spawnPoints =
                System.Array.Empty<Transform>();

            Assert.That(
                () =>
                    new RoundRobinSpawnPointSelector(
                        spawnPoints),
                Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithNullEntry_ReportsEntryIndex()
        {
            Transform validPoint =
                CreateSpawnPoint("Point_0");

            Transform[] spawnPoints =
            {
                validPoint,
                null
            };

            System.ArgumentException exception =
                Assert.Throws<System.ArgumentException>(
                    () =>
                        new RoundRobinSpawnPointSelector(
                            spawnPoints));

            Assert.That(
                exception.Message,
                Does.Contain("index 1"));
        }

        [Test]
        public void GetNext_WithMultiplePoints_ReturnsThemInOrder()
        {
            Transform firstPoint =
                CreateSpawnPoint("Point_0");

            Transform secondPoint =
                CreateSpawnPoint("Point_1");

            Transform thirdPoint =
                CreateSpawnPoint("Point_2");

            RoundRobinSpawnPointSelector selector =
                new RoundRobinSpawnPointSelector(
                    new[]
                    {
                        firstPoint,
                        secondPoint,
                        thirdPoint
                    });

            Assert.That(
                selector.GetNext(),
                Is.SameAs(firstPoint));

            Assert.That(
                selector.GetNext(),
                Is.SameAs(secondPoint));

            Assert.That(
                selector.GetNext(),
                Is.SameAs(thirdPoint));
        }

        [Test]
        public void GetNext_AfterLastPoint_WrapsToBeginning()
        {
            Transform firstPoint =
                CreateSpawnPoint("Point_0");

            Transform secondPoint =
                CreateSpawnPoint("Point_1");

            RoundRobinSpawnPointSelector selector =
                new RoundRobinSpawnPointSelector(
                    new[]
                    {
                        firstPoint,
                        secondPoint
                    });

            Assert.That(
                selector.GetNext(),
                Is.SameAs(firstPoint));

            Assert.That(
                selector.GetNext(),
                Is.SameAs(secondPoint));

            Assert.That(
                selector.GetNext(),
                Is.SameAs(firstPoint));

            Assert.That(
                selector.NextIndex,
                Is.EqualTo(1));
        }

        [Test]
        public void GetNext_WithSinglePoint_AlwaysReturnsSamePoint()
        {
            Transform onlyPoint =
                CreateSpawnPoint("OnlyPoint");

            RoundRobinSpawnPointSelector selector =
                new RoundRobinSpawnPointSelector(
                    new[]
                    {
                        onlyPoint
                    });

            Assert.That(
                selector.GetNext(),
                Is.SameAs(onlyPoint));

            Assert.That(
                selector.GetNext(),
                Is.SameAs(onlyPoint));

            Assert.That(
                selector.GetNext(),
                Is.SameAs(onlyPoint));

            Assert.That(
                selector.NextIndex,
                Is.EqualTo(0));
        }

        [Test]
        public void GetNext_WhenUpcomingPointWasDestroyed_Throws()
        {
            Transform firstPoint =
                CreateSpawnPoint("Point_0");

            Transform secondPoint =
                CreateSpawnPoint("Point_1");

            RoundRobinSpawnPointSelector selector =
                new RoundRobinSpawnPointSelector(
                    new[]
                    {
                        firstPoint,
                        secondPoint
                    });

            Assert.That(
                selector.GetNext(),
                Is.SameAs(firstPoint));

            Object.DestroyImmediate(
                secondPoint.gameObject);

            Assert.That(
                selector.GetNext,
                Throws.InvalidOperationException);
        }

        [Test]
        public void Constructor_CopiesSuppliedCollection()
        {
            Transform firstPoint =
                CreateSpawnPoint("Point_0");

            Transform secondPoint =
                CreateSpawnPoint("Point_1");

            List<Transform> source =
                new List<Transform>
                {
                    firstPoint
                };

            RoundRobinSpawnPointSelector selector =
                new RoundRobinSpawnPointSelector(source);

            source.Clear();
            source.Add(secondPoint);

            Assert.That(
                selector.Count,
                Is.EqualTo(1));

            Assert.That(
                selector.GetNext(),
                Is.SameAs(firstPoint));
        }

        private Transform CreateSpawnPoint(
            string objectName)
        {
            GameObject spawnPointObject =
                new GameObject(objectName);

            _createdObjects.Add(
                spawnPointObject);

            return spawnPointObject.transform;
        }
    }
}