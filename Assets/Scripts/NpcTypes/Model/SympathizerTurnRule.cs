using Project1028.Suspicion;

namespace Project1028.NpcTypes
{
    /// <summary>§3 동조자: 섬 의심도 구간이 turnZone 이상이면 감시자 규칙으로 전환. 순수 로직.</summary>
    public static class SympathizerTurnRule
    {
        public static bool IsTurned(AlertZone zone, AlertZone turnZone) => zone >= turnZone;
    }
}
