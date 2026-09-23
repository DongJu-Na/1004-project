namespace Project1028.NpcTypes
{
    /// <summary>§3 경계자: 감지 반경 안에 플레이어가 있고 쿨다운이 지났으면 알람. 순수 로직.</summary>
    public sealed class SentinelAlarmGate
    {
        private readonly float cooldown;
        private float nextAllowedAt = float.NegativeInfinity;

        public SentinelAlarmGate(float cooldownSeconds) { cooldown = cooldownSeconds < 0f ? 0f : cooldownSeconds; }

        public bool TryAlarm(bool playerInRadius, float now)
        {
            if (!playerInRadius) return false;
            if (now < nextAllowedAt) return false;
            nextAllowedAt = now + cooldown;
            return true;
        }
    }
}
