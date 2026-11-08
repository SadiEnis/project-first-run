using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.UI.Hud;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class CrosshairTests
    {
        [TestCase(WeaponDeliveryMode.Hitscan, 8, 0, CrosshairStyle.Spread)]
        [TestCase(WeaponDeliveryMode.Hitscan, 1, .35f, CrosshairStyle.Prepared)]
        [TestCase(WeaponDeliveryMode.Plasma, 1, 0, CrosshairStyle.Precision)]
        [TestCase(WeaponDeliveryMode.Rocket, 1, 0, CrosshairStyle.Projectile)]
        public void StyleFollowsDeliveryAndShotData(WeaponDeliveryMode delivery, int pellets,
            float preparation, CrosshairStyle expected)
            => Assert.That(CrosshairGeometry.Style(delivery, pellets, preparation), Is.EqualTo(expected));

        [Test]
        public void SpreadProjectionMatchesFovResolutionAndCanvasScaling()
        {
            float focal = 1 / Mathf.Tan(30 * Mathf.Deg2Rad); // 60 degree vertical FOV.
            float radius = CrosshairGeometry.SpreadRadius(6, focal, 1080, 1);
            Assert.That(radius, Is.EqualTo(98.31f).Within(.1f));
            Assert.That(CrosshairGeometry.SpreadRadius(6, focal, 2160, 2), Is.EqualTo(radius).Within(.001f));
            Assert.That(CrosshairGeometry.SpreadRadius(6, focal * 2, 1080, 1), Is.EqualTo(radius * 2).Within(.001f));
            Assert.That(CrosshairGeometry.SpreadRadius(0, focal, 1080, 1), Is.Zero);
        }

        [Test]
        public void SavedHudUsesSingleNonInteractiveCrosshairInsteadOfText()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/DemoHUD.prefab");
            var crosshairs = prefab.GetComponentsInChildren<HudCrosshair>(true);
            Assert.That(crosshairs, Has.Length.EqualTo(1));
            Assert.That(crosshairs[0].raycastTarget, Is.False);
            Assert.That(crosshairs[0].GetComponent<Text>(), Is.Null);
            var view = new SerializedObject(prefab.GetComponent<DemoHudView>());
            Assert.That(view.FindProperty("_crosshair").objectReferenceValue, Is.EqualTo(crosshairs[0]));
        }

        [Test]
        public void HitFeedbackRequiresAppliedDamageAndPreservesKillWithinVolley()
        {
            var go = new GameObject("crosshair-test", typeof(RectTransform), typeof(CanvasRenderer), typeof(HudCrosshair));
            try
            {
                var view = go.GetComponent<HudCrosshair>();
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var type = typeof(HudCrosshair);
                void Hit(DamageResult damage) => type.GetMethod("OnDamage", flags).Invoke(view,
                    new object[] { HitscanShotResult.HitDamageable(null, Vector3.zero, Vector3.forward, Vector3.one, damage) });
                float Timer() => (float)type.GetField("_hitTime", flags).GetValue(view);
                Hit(new DamageResult(10, 10, 10, 0, true));
                Assert.That(Timer(), Is.Zero, "Hidden HUD must not queue hit markers.");
                type.GetField("_visible", flags).SetValue(view, true);
                Hit(DamageResult.Rejected(10, 100));
                Assert.That(Timer(), Is.Zero);
                Hit(new DamageResult(10, 10, 10, 0, true));
                Hit(new DamageResult(10, 10, 100, 90, false));
                Assert.That(Timer(), Is.GreaterThan(0));
                Assert.That(type.GetField("_lethal", flags).GetValue(view), Is.EqualTo(true));
                view.Present(null, false);
                Assert.That(Timer(), Is.Zero);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
