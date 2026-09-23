using Project1028.PlayFoundation;

namespace Project1028.NpcTypes
{
    public struct SubdueVerdict
    {
        public bool CanSubdue;
        public bool PhysicalReaction;
        public string Reason;
    }

    /// <summary>§3.2: Antagonist → 제압 가능. Victim/Neutral → 불가, 물리 반응 없음. 순수 로직. 004가 소비.</summary>
    public static class SubdueJudgement
    {
        public static SubdueVerdict Evaluate(Faction faction)
        {
            if (faction == Faction.Antagonist)
                return new SubdueVerdict { CanSubdue = true, PhysicalReaction = true, Reason = "Antagonist (감시자·중개인)" };
            return new SubdueVerdict { CanSubdue = false, PhysicalReaction = false, Reason = $"{faction}: 제압 불가, 물리 반응 없음 (§3.2)" };
        }

        /// <summary>프로필이 없으면 Victim으로 취급 → 불가.</summary>
        public static SubdueVerdict Evaluate(NpcIdentity npc)
        {
            var profile = NpcTypeProfile.Get(npc);
            return Evaluate(profile != null ? profile.Faction : Faction.Victim);
        }
    }
}
