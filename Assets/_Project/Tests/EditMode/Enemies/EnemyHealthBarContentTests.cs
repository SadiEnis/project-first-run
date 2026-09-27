using NUnit.Framework;
using ProjectFirstRun.Enemies.Presentation;
using UnityEditor;
using UnityEngine;
namespace ProjectFirstRun.Tests.EditMode.Enemies
{
    public sealed class EnemyHealthBarContentTests
    {
        [Test] public void SharedEnemyPrefabIncludesOneHealthBar()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_ChaserBasic.prefab");
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponents<EnemyHealthBar>().Length, Is.EqualTo(1));
        }
    }
}
