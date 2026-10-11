using NUnit.Framework;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class WeaponAudioContentTests
    {
        [TestCase("Shotgun", 3)]
        [TestCase("Minigun", 3)]
        [TestCase("RocketLauncher", 1)]
        [TestCase("PlasmaRifle", 1)]
        public void WeaponsHaveExplicitAudioReferences(string name, int variants)
        {
            var definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                $"Assets/_Project/Data/Items/Weapons/WD_{name}.asset");
            Assert.That(definition, Is.Not.Null);
            Assert.That(definition.FireSounds, Has.Length.EqualTo(variants));
            foreach (var clip in definition.FireSounds)
            {
                Assert.That(clip, Is.Not.Null);
                Assert.That(clip.length, Is.GreaterThan(0));
                Assert.That(AssetDatabase.GetAssetPath(clip), Does.Not.Contain("/Resources/"));
            }
            Assert.That(definition.ReloadStartSound, Is.Not.Null);
            Assert.That(definition.ReloadEndSound, Is.Not.Null);
            Assert.That(definition.FireVolume, Is.InRange(.01f, 1f));
            Assert.That(definition.ReloadVolume, Is.InRange(.01f, 1f));
            Assert.DoesNotThrow(() => new PlayerWeaponRuntimeEntry(definition));
        }

        [Test]
        public void AudioIsOptionalForNewDefinitions()
        {
            var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            try
            {
                Assert.That(definition.FireSounds, Is.Empty);
                Assert.That(definition.ReloadStartSound, Is.Null);
                Assert.That(definition.ReloadEndSound, Is.Null);
            }
            finally { Object.DestroyImmediate(definition); }
        }
    }
}
