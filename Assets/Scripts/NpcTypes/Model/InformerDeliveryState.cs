namespace Project1028.NpcTypes
{
    public enum DeliveryPhase { Idle, Waiting, Moving, Held, Delivered }

    /// <summary>§3.1 밀고자 전달 상태기계. 플레이어별 인스턴스. 순수 로직.</summary>
    public sealed class InformerDeliveryState
    {
        public DeliveryPhase Phase { get; private set; } = DeliveryPhase.Idle;
        public float TriggeredAt { get; private set; }

        /// <summary>Idle에서만 Waiting으로 전환한다. 이동 중 재목격은 무시(Edge case).</summary>
        public bool Trigger(float now)
        {
            if (Phase != DeliveryPhase.Idle) return false;
            Phase = DeliveryPhase.Waiting;
            TriggeredAt = now;
            return true;
        }

        /// <summary>전이가 있으면 새 단계를 반환한다.</summary>
        public DeliveryPhase? Tick(float now, float delay, bool managerAvailable)
        {
            switch (Phase)
            {
                case DeliveryPhase.Waiting:
                    if (now - TriggeredAt < delay) return null;
                    Phase = managerAvailable ? DeliveryPhase.Moving : DeliveryPhase.Held;
                    return Phase;
                case DeliveryPhase.Held:
                    if (!managerAvailable) return null;
                    Phase = DeliveryPhase.Moving;
                    return Phase;
                case DeliveryPhase.Moving:
                    if (managerAvailable) return null;
                    Phase = DeliveryPhase.Held; // 이동 중 관리자가 사라짐
                    return Phase;
                default:
                    return null;
            }
        }

        public void Arrive()
        {
            if (Phase == DeliveryPhase.Moving) Phase = DeliveryPhase.Delivered;
        }

        public void Reset()
        {
            Phase = DeliveryPhase.Idle;
        }
    }
}
