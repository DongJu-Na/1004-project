using System.Collections.Generic;
using UnityEngine;

namespace Project1028.Slice
{
    public enum WaveKind { Sine, Square, Triangle }

    /// <summary>오디오 파일 없이 파형을 합성한다 (헌장 원칙 II). 심박·경보·저음·단음.</summary>
    [DisallowMultipleComponent]
    public sealed class ProceduralAudio : MonoBehaviour
    {
        public static ProceduralAudio Instance { get; private set; }

        private const int SampleRate = 44100;
        private AudioSource oneShot;
        private AudioSource drone;
        private AudioSource heart;
        private AudioClip heartClip;
        private float heartInterval;
        private float nextHeartAt;
        private float heartVolume = 0.5f;
        private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        private bool audioAvailable = true;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            try
            {
                oneShot = gameObject.AddComponent<AudioSource>();
                drone = gameObject.AddComponent<AudioSource>();
                heart = gameObject.AddComponent<AudioSource>();
                foreach (var s in new[] { oneShot, drone, heart }) { s.playOnAwake = false; s.spatialBlend = 0f; }
                drone.loop = true;
                drone.clip = Make("drone", 2f, t => 0.5f * Mathf.Sin(2f * Mathf.PI * 55f * t) + 0.25f * Mathf.Sin(2f * Mathf.PI * 110.5f * t), 1f, decay: false);
                drone.volume = 0.15f;
                heartClip = Make("heart", 0.35f, t => (t < 0.12f ? Mathf.Sin(2f * Mathf.PI * 60f * t) * (1f - t / 0.12f) : t > 0.18f && t < 0.3f ? 0.7f * Mathf.Sin(2f * Mathf.PI * 55f * (t - 0.18f)) * (1f - (t - 0.18f) / 0.12f) : 0f), 1f, decay: false);
            }
            catch (System.Exception ex) { audioAvailable = false; Debug.LogWarning("[ProceduralAudio] 오디오 사용 불가: " + ex.Message); }
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Update()
        {
            if (!audioAvailable || heartInterval <= 0f || heartClip == null) return;
            if (Time.unscaledTime >= nextHeartAt)
            {
                heart.PlayOneShot(heartClip, heartVolume);
                nextHeartAt = Time.unscaledTime + heartInterval;
            }
        }

        // ---------- 공개 API ----------

        public void Tone(float freq, float seconds, WaveKind wave = WaveKind.Sine, float volume = 0.4f)
        {
            if (!audioAvailable) return;
            string key = $"{wave}_{freq:0}_{seconds:0.00}";
            if (!cache.TryGetValue(key, out var clip))
            {
                clip = Make(key, seconds, t => Osc(wave, freq, t), 1f, decay: true);
                cache[key] = clip;
            }
            oneShot.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public void Noise(float seconds, float volume = 0.5f)
        {
            if (!audioAvailable) return;
            string key = $"noise_{seconds:0.00}";
            if (!cache.TryGetValue(key, out var clip))
            {
                var rng = new System.Random(7);
                clip = Make(key, seconds, t => (float)(rng.NextDouble() * 2.0 - 1.0), 1f, decay: true);
                cache[key] = clip;
            }
            oneShot.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public void Alarm()
        {
            if (!audioAvailable) return;
            Tone(880f, 0.2f, WaveKind.Square, 0.3f);
            StartCoroutine(Delayed(0.2f, () => Tone(660f, 0.2f, WaveKind.Square, 0.3f)));
        }

        public void Chord(bool rising)
        {
            if (!audioAvailable) return;
            float[] notes = rising ? new[] { 440f, 554f, 659f, 880f } : new[] { 659f, 554f, 440f, 330f };
            for (int i = 0; i < notes.Length; i++) { float f = notes[i]; StartCoroutine(Delayed(i * 0.12f, () => Tone(f, 0.35f, WaveKind.Triangle, 0.35f))); }
        }

        public void SetHeartbeat(float intervalSeconds, float volume)
        {
            heartInterval = intervalSeconds;
            heartVolume = Mathf.Clamp01(volume);
            if (intervalSeconds <= 0f) nextHeartAt = float.MaxValue;
            else if (nextHeartAt == float.MaxValue) nextHeartAt = Time.unscaledTime;
        }

        public void SetDrone(bool on)
        {
            if (!audioAvailable || drone == null) return;
            if (on && !drone.isPlaying) drone.Play();
            else if (!on && drone.isPlaying) drone.Stop();
        }

        // ---------- 합성 ----------

        private static float Osc(WaveKind w, float f, float t)
        {
            float ph = (f * t) % 1f;
            switch (w)
            {
                case WaveKind.Square: return ph < 0.5f ? 1f : -1f;
                case WaveKind.Triangle: return 4f * Mathf.Abs(ph - 0.5f) - 1f;
                default: return Mathf.Sin(2f * Mathf.PI * f * t);
            }
        }

        private static AudioClip Make(string name, float seconds, System.Func<float, float> gen, float amp, bool decay)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(seconds * SampleRate));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                float env = decay ? Mathf.Exp(-4f * t / seconds) : 1f;
                float attack = decay ? Mathf.Clamp01(i / 200f) : 1f; // 루프 클립은 램프 없이(이음새 클릭 방지)
                data[i] = Mathf.Clamp(gen(t) * amp * env * attack, -1f, 1f);
            }
            var clip = AudioClip.Create(name, n, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private System.Collections.IEnumerator Delayed(float seconds, System.Action a)
        {
            yield return new WaitForSecondsRealtime(seconds);
            a?.Invoke();
        }
    }
}
