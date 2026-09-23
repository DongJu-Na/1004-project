using UnityEngine;

namespace Project1028.Report
{
    public enum CutTickResult { Idle, Progressed, Cancelled, Completed }

    /// <summary>§5.2 "앞질러 간다": 전화선 끊기 홀드 진행. 유효 조건이 깨지면 취소(진행 0). 순수 로직.</summary>
    public sealed class CutProgress
    {
        private readonly float duration;
        private float elapsed;

        public CutProgress(float durationSeconds) { duration = Mathf.Max(0.01f, durationSeconds); }

        public float Progress01 => Mathf.Clamp01(elapsed / duration);
        public bool IsDone => elapsed >= duration;

        public CutTickResult Tick(float dt, bool valid)
        {
            if (IsDone) return CutTickResult.Idle;
            if (!valid)
            {
                bool hadProgress = elapsed > 0f;
                elapsed = 0f;
                return hadProgress ? CutTickResult.Cancelled : CutTickResult.Idle;
            }
            elapsed += Mathf.Max(0f, dt);
            return IsDone ? CutTickResult.Completed : CutTickResult.Progressed;
        }

        public void Reset() => elapsed = 0f;
    }
}
