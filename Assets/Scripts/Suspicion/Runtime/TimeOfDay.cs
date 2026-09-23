using System;
using UnityEngine;

namespace Project1028.Suspicion
{
    public enum DayPhase { Day, Night }

    /// <summary>§7 낮·밤 상태. 자동 전환은 후속(시계·출항 시각). 인스턴스가 없으면 Day로 취급.</summary>
    [DisallowMultipleComponent]
    public sealed class TimeOfDay : MonoBehaviour
    {
        public static TimeOfDay Instance { get; private set; }
        public static DayPhase CurrentOrDay => Instance != null ? Instance.Phase : DayPhase.Day;

        [SerializeField] private DayPhase phase = DayPhase.Day;

        public DayPhase Phase => phase;
        public event Action<DayPhase> OnChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Set(DayPhase newPhase)
        {
            if (phase == newPhase) return;
            phase = newPhase;
            OnChanged?.Invoke(phase);
        }

        public void Toggle() => Set(phase == DayPhase.Day ? DayPhase.Night : DayPhase.Day);
    }
}
