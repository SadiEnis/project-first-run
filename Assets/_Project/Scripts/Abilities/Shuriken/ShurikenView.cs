using System;
using ProjectFirstRun.Combat;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Shuriken
{
    public sealed class ShurikenView : MonoBehaviour
    {
        private GameObject _source;
        private Material _material;
        private Transform[] _blades;
        public Action Cancelled;
        public static ShurikenView Create(GameObject source, Material template, int count, UnityEngine.SceneManagement.Scene scene)
        {
            var go = new GameObject("Shuriken orbit");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
            var view = go.AddComponent<ShurikenView>(); view._source = source;
            view._material = new Material(template);
            if (view._material.HasProperty("_BaseColor")) view._material.SetColor("_BaseColor", new Color(.8f, .9f, 1));
            view._blades = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                var blade = new GameObject("Shuriken"); blade.transform.SetParent(go.transform, false);
                view._blades[i] = blade.transform;
                for (int j = 0; j < 2; j++)
                {
                    var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    arm.transform.SetParent(blade.transform, false);
                    arm.transform.localScale = new Vector3(.65f, .08f, .15f);
                    arm.transform.localRotation = Quaternion.Euler(0, 90 * j, 0);
                    var collider = arm.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
                    arm.GetComponent<Renderer>().sharedMaterial = view._material;
                }
            }
            return view;
        }
        public void Position(int index, Vector3 position, float angle)
        {
            _blades[index].position = position;
            _blades[index].rotation = Quaternion.Euler(0, angle * 4, 0);
        }
        private void Update()
        {
            var control = _source != null ? _source.GetComponent<PlayerAbilityController>() : null;
            var health = _source != null ? _source.GetComponent<HealthComponent>() : null;
            if (_source == null || !_source.activeInHierarchy || (health != null && health.IsDead) ||
                (control != null && (!control.isActiveAndEnabled || !control.IsAbilityControlEnabled)))
            {
                Cancelled?.Invoke();
                if (this != null) { gameObject.SetActive(false); Destroy(gameObject); }
            }
        }
        private void OnDestroy()
        {
            Cancelled?.Invoke();
            if (_material != null) Destroy(_material);
        }
    }
}
