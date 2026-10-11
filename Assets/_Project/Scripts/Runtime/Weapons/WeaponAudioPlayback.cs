using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    // Presentation only: bounded voices, no damage/ammo/timing authority.
    internal sealed class WeaponAudioPlayback
    {
        private readonly Transform _owner;
        private GameObject _root;
        private AudioSource[] _shots;
        private AudioSource _reload;
        private int _voice;
        private int _variant;
        private bool _paused;

        public WeaponAudioPlayback(Transform owner) => _owner = owner;

        private void EnsureSources()
        {
            if (_root != null) return;
            _root = new GameObject("Weapon audio (runtime)");
            _root.transform.SetParent(_owner, false);
            _shots = new AudioSource[8];
            for (int i = 0; i < _shots.Length; i++) _shots[i] = CreateSource();
            _reload = CreateSource();
        }

        private AudioSource CreateSource()
        {
            var source = _root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0; // Local player's weapon, not world/AI audio.
            source.dopplerLevel = 0;
            return source;
        }

        public void Fire(WeaponDefinition definition)
        {
            var clips = definition.FireSounds;
            if (clips == null || clips.Length == 0) return;
            var clip = clips[_variant++ % clips.Length];
            if (clip == null) return;
            EnsureSources();
            var source = _shots[_voice++ % _shots.Length];
            source.Stop();
            source.clip = clip;
            source.volume = definition.FireVolume;
            source.Play();
        }

        public void Reload(WeaponDefinition definition, bool completed)
        {
            if (_reload != null) _reload.Stop();
            var clip = completed ? definition.ReloadEndSound : definition.ReloadStartSound;
            if (clip == null) return;
            EnsureSources();
            _reload.clip = clip;
            _reload.volume = definition.ReloadVolume;
            _reload.Play();
        }

        public void SetPaused(bool paused)
        {
            if (_root == null || _paused == paused) return;
            _paused = paused;
            foreach (var source in _shots)
                if (paused) source.Pause(); else source.UnPause();
            if (paused) _reload.Pause(); else _reload.UnPause();
        }

        public void Stop()
        {
            if (_root == null) return;
            foreach (var source in _shots) source.Stop();
            _reload.Stop();
            _paused = false;
        }
    }
}
