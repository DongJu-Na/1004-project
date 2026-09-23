namespace Project1028.Subdue
{
    /// <summary>§4.2 손 제약. 제압에는 두 손이 필요하다 → 무엇이든 들고 있으면 뒤에서 제압·밀치기 불가. 물건으로 기절은 물건을 들어야 가능.</summary>
    public static class HandRules
    {
        public static (bool ok, string reason) CanPerform(SubdueAction action, HandState hands, HeldKind held)
        {
            if (held == HeldKind.UnconsciousNpc) return (false, "기절자를 들고 있어 두 손이 점유됨");
            switch (action)
            {
                case SubdueAction.Backstab:
                case SubdueAction.Shove:
                    return hands == HandState.Empty ? (true, string.Empty) : (false, "두 손이 필요하다 — 들고 있는 것을 내려놓아야 함 (§4.2)");
                case SubdueAction.ObjectStrike:
                    return held == HeldKind.Object ? (true, string.Empty) : (false, "물건을 들고 있어야 함 (§4.1)");
                default:
                    return (false, "알 수 없는 동작");
            }
        }

        public static bool CanPickUpObject(HandState hands) => hands == HandState.Empty;
        public static bool CanPickUpBody(HandState hands) => hands == HandState.Empty;
    }
}
