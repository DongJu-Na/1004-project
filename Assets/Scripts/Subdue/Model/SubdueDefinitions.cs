namespace Project1028.Subdue
{
    /// <summary>§4.1 세 동작. 모두 비살상. 무기 개념은 존재하지 않는다 (헌장 원칙 V).</summary>
    public enum SubdueAction { Backstab, Shove, ObjectStrike }

    /// <summary>§4.2 손 상태.</summary>
    public enum HandState { Empty, OneHand, TwoHands }

    public enum HeldKind { None, Object, UnconsciousNpc }

    /// <summary>§4.1 소음 등급.</summary>
    public enum NoiseLevel { Low, Medium, High }

    public static class SubdueNames
    {
        public static string Korean(SubdueAction a)
        {
            switch (a)
            {
                case SubdueAction.Backstab: return "뒤에서 제압";
                case SubdueAction.Shove: return "정면 밀치기";
                default: return "물건으로 기절";
            }
        }

        public static string Korean(NoiseLevel n)
        {
            switch (n)
            {
                case NoiseLevel.Low: return "낮음";
                case NoiseLevel.Medium: return "중간";
                default: return "높음";
            }
        }

        public static string Korean(HandState h)
        {
            switch (h)
            {
                case HandState.Empty: return "빈손";
                case HandState.OneHand: return "한 손";
                default: return "두 손";
            }
        }
    }
}
