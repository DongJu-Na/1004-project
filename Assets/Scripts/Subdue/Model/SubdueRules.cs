using System;

namespace Project1028.Subdue
{
    /// <summary>subdue_rules.json 모델. 스키마: contracts/subdue-rules-schema.json</summary>
    [Serializable]
    public sealed class SubdueRules
    {
        public string version;
        public ActionRuleSet actions;
        public UnconsciousRule unconscious;
        public CarryRule carry;
        public CostRule costs;

        public ActionRule ForAction(SubdueAction a)
        {
            if (actions == null) return null;
            switch (a)
            {
                case SubdueAction.Backstab: return actions.backstab;
                case SubdueAction.Shove: return actions.shove;
                default: return actions.objectStrike;
            }
        }
    }

    [Serializable] public sealed class ActionRuleSet { public ActionRule backstab; public ActionRule shove; public ActionRule objectStrike; }
    [Serializable] public sealed class ActionRule { public float range; public float backConeDegrees; public float failChance; public string noiseLevel; public string noiseEventId; public string status; }
    [Serializable] public sealed class UnconsciousRule { public float minSeconds; public float maxSeconds; public string status; }
    [Serializable] public sealed class CarryRule { public float speedMultiplier; public string status; }
    [Serializable] public sealed class CostRule { public string witnessedEventId; public string foundIslandEventId; public string foundPersonalEventId; public string wakeEventId; public string shoveFailEventId; public string status; }
}
