namespace Project1028.Encounter
{
    /// <summary>§6.4 스폰 규격 게이트. 순수 로직.</summary>
    public static class SpawnGate
    {
        public struct SpawnContext
        {
            public int RunCount, PerRunMax;
            public float SinceLastEnd, MinInterval;
            public bool Moving, Busy, Scouting, ActiveInstance;
        }

        public static (bool ok, string reason) CanSpawn(SpawnContext c)
        {
            if (c.ActiveInstance) return (false, "진행 중인 인카운터");
            if (c.RunCount >= c.PerRunMax) return (false, "런당 상한");
            if (c.SinceLastEnd < c.MinInterval) return (false, "최소 간격");
            if (c.Scouting) return (false, "조사·정찰 중");
            if (c.Busy) return (false, "바쁨(대화·잠금·두 손)");
            if (!c.Moving) return (false, "이동 중 아님");
            return (true, "가능");
        }

        public static bool Roll(bool inVehicle, float footChance, float vehicleChance, float roll01) =>
            roll01 < (inVehicle ? vehicleChance : footChance);
    }
}
